namespace BookManagement.Api.Endpoints;

public static class UploadEndpoints
{
    private const long MaxUploadSizeBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif"
    };

    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/upload", async (IFormFile file, IWebHostEnvironment environment) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { message = "File is empty." });
            }

            if (file.Length > MaxUploadSizeBytes)
            {
                return Results.BadRequest(new { message = "File is too large." });
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                return Results.BadRequest(new { message = "Unsupported file type." });
            }

            var apiImagesPath = Path.Combine(
                environment.ContentRootPath,
                "wwwroot",
                "images",
                "books");

            Directory.CreateDirectory(apiImagesPath);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(apiImagesPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream);

            return Results.Ok(new { url = $"/api/images/books/{fileName}" });
        })
        .DisableAntiforgery()
        .RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
