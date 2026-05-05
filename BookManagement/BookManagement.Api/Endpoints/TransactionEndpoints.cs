using System.Security.Claims;
using BookManagement.Api.Models;
using BookManagement.Api.Validation;
using BookManagement.Core.Entities;
using BookManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Api.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var transactions = endpoints.MapGroup("/api/transactions").RequireAuthorization("UserOrAdmin");

        // POST /api/transactions - Mua sách trực tiếp
        transactions.MapPost("/", async Task<IResult>(
            CreateTransactionRequest request,
            ClaimsPrincipal principal,
            AppDbContext db) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            if (request.Type != TransactionType.Buy)
            {
                return Results.BadRequest(new { message = "Chi ho tro giao dich mua truc tiep." });
            }

            var book = await db.Books.FindAsync(request.BookId);
            if (book is null)
            {
                return Results.NotFound(new { message = "Khong tim thay sach." });
            }

            var user = await db.Users.FindAsync(userId);
            if (user is null || string.Equals(user.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            var bookPrice = book.Price ?? 0m;
            if (bookPrice <= 0)
            {
                return Results.BadRequest(new { message = "Sach chua co gia hop le." });
            }

            var alreadyOwned = await db.Transactions.AnyAsync(x =>
                x.BookId == request.BookId &&
                x.Type == TransactionType.Buy);
            if (alreadyOwned)
            {
                return Results.Conflict(new { message = "Sách này đã được bán." });
            }

            if (user.Balance < bookPrice)
            {
                return Results.BadRequest(new { message = "So du khong du de mua sach." });
            }

            await using var trx = await db.Database.BeginTransactionAsync();
            try
            {
                user.Balance -= bookPrice;
                db.Transactions.Add(new BookTransaction
                {
                    BookId = request.BookId,
                    UserId = userId,
                    Type = TransactionType.Buy,
                    Amount = bookPrice,
                    TransactionDate = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
                await trx.CommitAsync();

                return Results.Ok(new
                {
                    bookId = request.BookId,
                    amount = bookPrice,
                    balance = user.Balance
                });
            }
            catch
            {
                await trx.RollbackAsync();
                return Results.Problem("Khong the hoan tat giao dich mua sach.");
            }
        });

        // POST /api/transactions/deposit - Nạp tiền vào ví
        transactions.MapPost("/deposit", async Task<IResult>(
            DepositRequest request,
            ClaimsPrincipal principal,
            AppDbContext db) =>
        {
            var validationErrors = request.Validate();
            if (validationErrors is not null)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var user = await db.Users.FindAsync(userId);
            if (user is null || string.Equals(user.Status, "Deleted", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            user.Balance += request.Amount;
            db.Transactions.Add(new BookTransaction
            {
                UserId = userId,
                Type = TransactionType.Deposit,
                Amount = request.Amount,
                TransactionDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Nap tien thanh cong.", balance = user.Balance });
        });

        // GET /api/transactions/all - Admin lấy tất cả giao dịch
        transactions.MapGet("/all", async Task<IResult>(AppDbContext db) =>
        {
            var transactionsList = await db.Transactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Book)
                .OrderByDescending(x => x.TransactionDate)
                .Select(t => new
                {
                    t.Id,
                    t.Type,
                    t.Amount,
                    t.TransactionDate,
                    t.BookId,
                    User = new { t.User.Email },
                    Book = t.Book != null ? new { t.Book.Title } : null
                })
                .ToListAsync();

            return Results.Ok(transactionsList);
        }).RequireAuthorization("AdminOnly");

        // GET /api/transactions/sold - Public, lấy ID các sách đã bán
        transactions.MapGet("/sold", async Task<IResult>(AppDbContext db) =>
        {
            var soldIds = await db.Transactions
                .Where(t => t.Type == TransactionType.Buy && t.BookId != null)
                .Select(t => t.BookId)
                .Distinct()
                .ToListAsync();
            return Results.Ok(soldIds);
        }).AllowAnonymous();

        // GET /api/transactions/purchased - Lấy sách đã mua của user hiện tại
        transactions.MapGet("/purchased", async Task<IResult>(
            ClaimsPrincipal principal,
            AppDbContext db) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var purchasedBooks = await db.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Type == TransactionType.Buy && x.BookId != null)
                .OrderByDescending(x => x.TransactionDate)
                .Join(
                    db.Books.AsNoTracking(),
                    tx => tx.BookId,
                    book => book.Id,
                    (tx, book) => new
                    {
                        tx.BookId,
                        tx.TransactionDate,
                        tx.Amount,
                        Book = book
                    })
                .GroupBy(x => x.BookId)
                .Select(g => g.OrderByDescending(x => x.TransactionDate).First())
                .ToListAsync();

            return Results.Ok(purchasedBooks.Select(x => new
            {
                id = x.Book.Id,
                title = x.Book.Title,
                author = x.Book.Author,
                coverUrl = x.Book.CoverUrl,
                price = x.Book.Price,
                purchasedAt = x.TransactionDate
            }));
        });

        // GET /api/transactions/my - Lịch sử giao dịch của user hiện tại
        transactions.MapGet("/my", async Task<IResult>(
            ClaimsPrincipal principal,
            AppDbContext db) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Results.Unauthorized();
            }

            var items = await db.Transactions
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.TransactionDate)
                .Take(100)
                .Select(x => new
                {
                    x.Id,
                    x.Type,
                    x.Amount,
                    x.TransactionDate,
                    x.BookId
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        return endpoints;
    }
}
