using Microsoft.AspNetCore.Mvc;

namespace BookManagement.Web.Endpoints;

public static class ApiProxyEndpoints
{
    public static IEndpointRouteBuilder MapApiProxy(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api");

        // Forward tất cả các request đến /api/* sang backend API
        group.Map("/{**catch-all}", async (HttpContext context, IHttpClientFactory httpClientFactory, IConfiguration config) =>
        {
            var baseUrl = config["Api:BaseUrl"] ?? "http://localhost:5104";
            var client = httpClientFactory.CreateClient("BookManagementApi");

            using var requestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{baseUrl}{context.Request.Path}{context.Request.QueryString}"),
                Method = new HttpMethod(context.Request.Method)
            };

            // Chuyển tiếp headers, loại bỏ Host để backend xử lý đúng
            foreach (var header in context.Request.Headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) ||
                    header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // Nếu có body thì chuyển tiếp — MemoryStream phải tồn tại tới khi SendAsync hoàn tất
            MemoryStream? bodyBuffer = null;
            if (context.Request.ContentLength > 0 || context.Request.Headers.TransferEncoding.Count > 0)
            {
                bodyBuffer = new MemoryStream();
                await context.Request.Body.CopyToAsync(bodyBuffer, context.RequestAborted);
                bodyBuffer.Position = 0;
                requestMessage.Content = new StreamContent(bodyBuffer);

                if (context.Request.ContentType != null)
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", context.Request.ContentType);
                }
                if (context.Request.ContentLength > 0)
                {
                    requestMessage.Content.Headers.TryAddWithoutValidation("Content-Length", context.Request.ContentLength.ToString());
                }
            }

            try
            {
                using var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

                context.Response.StatusCode = (int)response.StatusCode;

                // Các header bị cấm không được ghi lại vào response proxy
                var excludedHeaders = new[] { "Transfer-Encoding", "Connection", "Keep-Alive" };

                foreach (var header in response.Headers)
                {
                    if (excludedHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase)) continue;
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
                foreach (var header in response.Content.Headers)
                {
                    if (excludedHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase)) continue;
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
            catch (HttpRequestException)
            {
                // API không chạy hoặc từ chối kết nối
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "Không thể kết nối tới máy chủ API. Vui lòng thử lại sau." });
            }
            catch (TaskCanceledException)
            {
                // Request bị timeout hoặc user hủy
                if (!context.RequestAborted.IsCancellationRequested)
                {
                    context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Yêu cầu tới máy chủ API đã hết thời gian chờ." });
                }
            }
            finally
            {
                if (bodyBuffer != null)
                {
                    await bodyBuffer.DisposeAsync();
                }
            }
        });

        return endpoints;
    }
}
