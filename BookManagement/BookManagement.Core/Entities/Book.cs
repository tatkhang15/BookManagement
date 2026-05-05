namespace BookManagement.Core.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime? PublishDate { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Genres { get; set; } = string.Empty; // Lưu dạng "Tiểu thuyết,Khoa học,Lịch sử"
    public string CoverUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? PageCount { get; set; }
}
