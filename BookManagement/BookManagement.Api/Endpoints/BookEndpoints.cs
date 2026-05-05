using BookManagement.Api.Models;
using BookManagement.Api.Validation;
using BookManagement.Core.Entities;
using BookManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var books = endpoints.MapGroup("/api/books");

        books.MapGet("/", async (AppDbContext db) =>
        {
            var data = await db.Books
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .ToListAsync();
            return Results.Ok(data);
        });

        books.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return book is null ? Results.NotFound() : Results.Ok(book);
        });

        books.MapPost("/", async (BookRequest request, AppDbContext db) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var normalizedIsbn = request.Isbn.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedIsbn)
                && await db.Books.AnyAsync(x => x.Isbn == normalizedIsbn))
            {
                return Results.Conflict(new { message = "ISBN da ton tai." });
            }

            var book = new Book
            {
                Title = request.Title.Trim(),
                Author = request.Author.Trim(),
                Genres = request.Genres.Trim(),
                Isbn = normalizedIsbn,
                Price = request.Price,
                CoverUrl = request.CoverUrl.Trim(),
                PublishDate = request.PublishDate,
                Description = request.Description.Trim(),
                PageCount = request.PageCount
            };

            db.Books.Add(book);
            await db.SaveChangesAsync();
            return Results.Created($"/api/books/{book.Id}", book);
        }).RequireAuthorization("AdminOnly");

        books.MapPut("/{id:int}", async (int id, BookRequest request, AppDbContext db) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var book = await db.Books.FindAsync(id);
            if (book is null)
            {
                return Results.NotFound();
            }

            var normalizedIsbn = request.Isbn.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedIsbn)
                && await db.Books.AnyAsync(x => x.Id != id && x.Isbn == normalizedIsbn))
            {
                return Results.Conflict(new { message = "ISBN da ton tai." });
            }

            book.Title = request.Title.Trim();
            book.Author = request.Author.Trim();
            book.Genres = request.Genres.Trim();
            book.Isbn = normalizedIsbn;
            book.Price = request.Price;
            book.CoverUrl = request.CoverUrl.Trim();
            book.PublishDate = request.PublishDate;
            book.Description = request.Description.Trim();
            book.PageCount = request.PageCount;

            await db.SaveChangesAsync();
            return Results.Ok(book);
        }).RequireAuthorization("AdminOnly");

        books.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null)
            {
                return Results.NotFound();
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
