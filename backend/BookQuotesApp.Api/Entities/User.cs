namespace BookQuotesApp.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
