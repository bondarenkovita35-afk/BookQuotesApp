namespace BookQuotesApp.Api.Entities;

public class Quote
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
