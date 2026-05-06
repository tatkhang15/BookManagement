using System.Text;
using System.Text.Json;
using BookManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace BookManagement.Api.Endpoints;

public static class ChatEndpoints
{
    private static readonly ConcurrentDictionary<string, (DateTime timestamp, string response)> _cache = new();
    private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    private static readonly ConcurrentDictionary<string, (DateTime windowStart, int count)> _rateLimit = new();
    private static readonly TimeSpan _rateLimitWindow = TimeSpan.FromMinutes(1);
    private const int _maxRequestsPerWindowPerIp = 10;

    private sealed record BookLite(string Title, string Author, string Genres, decimal? Price, int? PageCount);
    
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/chat", async (
            ChatRequest request,
            AppDbContext db,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            HttpContext httpContext) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { reply = "Vui lòng nhập câu hỏi." });
            }

            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (IsRateLimited(ip))
            {
                return Results.Ok(new { reply = "Bạn nhắn hơi nhanh 😅 Bạn chờ 1 chút rồi thử lại nhé." });
            }

            // Cache check - trả về câu trả lời đã có nếu trùng câu hỏi
            var normalizedMessage = request.Message.Trim().ToLowerInvariant();
            if (_cache.TryGetValue(normalizedMessage, out var cached) && DateTime.Now - cached.timestamp < _cacheExpiry)
            {
                return Results.Ok(new { reply = cached.response });
            }

            var apiKey = config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Results.Problem("Gemini API Key chưa được cấu hình.");
            }

            // Lấy danh sách sách từ database để Gemini biết kho sách hiện tại (giới hạn 5 sách để giảm token)
            var books = await db.Books
                .AsNoTracking()
                .OrderByDescending(b => b.Id)
                .Take(5)
                .Select(b => new BookLite(
                    b.Title,
                    b.Author,
                    b.Genres,
                    b.Price,
                    b.PageCount))
                .ToListAsync();

            var bookList = books.Count > 0
                ? string.Join("\n", books.Select(b =>
                    $"- \"{b.Title}\" | {b.Author} | {b.Genres} | {(b.Price.HasValue ? $"{b.Price:N0}đ" : "Giá chưa cập nhật")}"))
                : "Hiện tại chưa có sách nào trong hệ thống.";

            var systemPrompt = $"""
                Bạn là trợ lý AI của hệ thống sách. Trả lời ngắn gọn bằng tiếng Việt.
                
                Sách có sẵn:
                {bookList}
                
                Trả lời tối đa 50 chữ. Dùng emoji. Nếu không có sách phù hợp, nói rõ.
                """;

            // Gọi Gemini API
            var client = httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            var geminiPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = $"{systemPrompt}\n\nCâu hỏi của khách: {request.Message}" } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    maxOutputTokens = 150
                }
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(geminiPayload, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return Results.Ok(new { reply = "Gemini API Key không hợp lệ hoặc bị chặn quyền. Bạn kiểm tra lại key/quyền truy cập trong Google AI Studio nhé." });
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        // Fallback response khi quá tải
                        return Results.Ok(new { reply = "Gemini đang bị giới hạn (429 / hết quota hoặc rate limit). Bạn thử lại sau, hoặc dùng API key khác / tăng quota nhé." });
                    }
                    return Results.Problem($"Gemini API lỗi ({response.StatusCode}): {responseBody}");
                }

                // Parse kết quả từ Gemini an toàn hơn
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var candidateContent) && 
                        candidateContent.TryGetProperty("parts", out var parts) && 
                        parts.GetArrayLength() > 0 &&
                        parts[0].TryGetProperty("text", out var textProp))
                    {
                        var reply = textProp.GetString() ?? "Xin lỗi, tôi không thể trả lời lúc này.";
                        
                        // Cache response để dùng lại sau
                        _cache.TryAdd(normalizedMessage, (DateTime.Now, reply));
                        
                        return Results.Ok(new { reply });
                    }
                    else if (firstCandidate.TryGetProperty("finishReason", out var finishReason) && 
                             finishReason.GetString() == "SAFETY")
                    {
                        return Results.Ok(new { reply = "Xin lỗi, câu hỏi của bạn có thể chứa nội dung không phù hợp nên tôi không thể trả lời. 😔" });
                    }
                }

                return Results.Ok(new { reply = "Xin lỗi, tôi không thể xử lý câu trả lời từ hệ thống lúc này. Bạn vui lòng thử lại sau nhé. 😔" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Lỗi kết nối Gemini: {ex.Message}");
            }
        }).AllowAnonymous();

        return endpoints;
    }

    private static bool IsRateLimited(string ip)
    {
        var now = DateTime.UtcNow;
        while (true)
        {
            if (!_rateLimit.TryGetValue(ip, out var state))
            {
                if (_rateLimit.TryAdd(ip, (now, 1)))
                    return false;
                continue;
            }

            if (now - state.windowStart >= _rateLimitWindow)
            {
                if (_rateLimit.TryUpdate(ip, (now, 1), state))
                    return false;
                continue;
            }

            if (state.count >= _maxRequestsPerWindowPerIp)
                return true;

            if (_rateLimit.TryUpdate(ip, (state.windowStart, state.count + 1), state))
                return false;
        }
    }

    private static string GetFallbackResponse(string message, List<BookLite> books)
    {
        var lowerMessage = message.ToLowerInvariant();
        
        if (lowerMessage.Contains("xin chào") || lowerMessage.Contains("hello"))
            return "Xin chào! Tôi có thể giúp gì cho bạn về sách? 📚";
            
        if (lowerMessage.Contains("sách") && books.Count > 0)
        {
            var randomBook = books[new Random().Next(books.Count)];
            return $"Hiện có sách \"{randomBook.Title}\" của {randomBook.Author}. Bạn quan tâm không? 📖";
        }
        
        if (lowerMessage.Contains("giá") || lowerMessage.Contains("bao nhiêu"))
            return "Vui lòng cho biết tên sách để tôi báo giá nhé! 💰";
            
        return "Xin lỗi, hệ thống đang bận. Bạn thử lại sau ít phút nhé! ⏳";
    }

    public record ChatRequest(string Message);
}
