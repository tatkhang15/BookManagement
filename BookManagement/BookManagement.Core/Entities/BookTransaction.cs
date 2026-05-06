namespace BookManagement.Core.Entities;

public enum TransactionType
{
    Buy,
    Deposit
}

public class BookTransaction
{
    public int Id { get; set; }
    public int? BookId { get; set; }
    public Book? Book { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public TransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
}
