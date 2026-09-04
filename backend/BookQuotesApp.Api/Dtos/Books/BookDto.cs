namespace BookQuotesApp.Api.Dtos.Books;

public record BookDto(int Id, string Title, string Author, DateOnly PublishedDate);
