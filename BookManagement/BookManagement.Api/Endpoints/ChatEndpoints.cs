using System.Text;
using System.Text.Json;
using BookManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Api.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/chat", async (
            ChatRequest request,
            AppDbContext db,
            IConfiguration config,
            IHttpClientFactory httpClientFactory) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest(new { reply = "Vui lòng nhập câu hỏi." });
            }

            var apiKey = config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return Results.Problem("Gemini API Key chưa được cấu hình.");
            }

            // Lấy danh sách sách từ database để Gemini biết kho sách hiện tại
            var books = await db.Books
                .AsNoTracking()
                .OrderByDescending(b => b.Id)
                .Take(100)
                .Select(b => new
                {
                    b.Title,
                    b.Author,
                    b.Genres,
                    b.Price,
                    b.Isbn,
                    b.Description,
                    b.PageCount
                })
                .ToListAsync();

            var bookList = books.Count > 0
                ? string.Join("\n", books.Select(b =>
                    $"- \"{b.Title}\" | Tác giả: {b.Author} | Thể loại: {b.Genres} | Giá: {(b.Price.HasValue ? $"{b.Price:N0} VNĐ" : "Chưa cập nhật")} | Số trang: {b.PageCount?.ToString() ?? "N/A"} | ISBN: {b.Isbn ?? "N/A"} | Mô tả: {(string.IsNullOrWhiteSpace(b.Description) ? "Chưa có" : b.Description)}"))
                : "Hiện tại chưa có sách nào trong hệ thống.";

            var systemPrompt = $"""
                Bạn là trợ lý AI thông minh của hệ thống quản lý sách "Book Management".
                Bạn thân thiện, nhiệt tình và trả lời bằng tiếng Việt.
                
                Dưới đây là danh sách sách hiện có trong cửa hàng:
                {bookList}
                
                Hãy trả lời câu hỏi của khách hàng dựa trên danh sách sách trên.
                Nếu khách hỏi về sách không có trong danh sách, hãy cho biết cửa hàng chưa có sách đó.
                Nếu khách hỏi chung chung, hãy gợi ý một vài cuốn sách hay từ danh sách.
                Trả lời ngắn gọn, súc tích, dễ hiểu. Dùng emoji cho sinh động.
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
                    temperature = 0.7,
                    maxOutputTokens = 1024
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
                    return Results.Problem($"Gemini API lỗi: {response.StatusCode}");
                }

                // Parse kết quả từ Gemini
                using var doc = JsonDocument.Parse(responseBody);
                var reply = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Xin lỗi, tôi không thể trả lời lúc này.";

                return Results.Ok(new { reply });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Lỗi kết nối Gemini: {ex.Message}");
            }
        }).AllowAnonymous();

        return endpoints;
    }

    public record ChatRequest(string Message);
}
