using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BookManagement.Core.Entities;
using BookManagement.Api.Serialization;

namespace BookManagement.Api.Models;

public sealed class BookRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 2)]
    public string Author { get; set; } = string.Empty;

    [JsonConverter(typeof(NullableDateTimeJsonConverter))]
    public DateTime? PublishDate { get; set; }
    [StringLength(32)]
    public string Isbn { get; set; } = string.Empty;
    [Range(0.01, 1000000000)]
    public decimal? Price { get; set; }
    [StringLength(500)]
    public string Genres { get; set; } = string.Empty;
    [StringLength(1000)]
    public string CoverUrl { get; set; } = string.Empty;
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;
    [Range(1, 50000)]
    public int? PageCount { get; set; }
}

public sealed class CreateTransactionRequest
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public TransactionType Type { get; set; }
}

public sealed class DepositRequest
{
    [Range(1, 1000000000)]
    public decimal Amount { get; set; }
}
