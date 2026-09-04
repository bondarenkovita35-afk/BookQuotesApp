using BookQuotesApp.Api.Common;
using BookQuotesApp.Api.Data;
using BookQuotesApp.Api.Dtos.Books;
using BookQuotesApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookQuotesApp.Api.Services;

public class BookService(AppDbContext db) : IBookService
{
    public async Task<IReadOnlyList<BookDto>> GetAllAsync(int userId)
    {
        return await db.Books
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => ToDto(b))
            .ToListAsync();
    }

    public async Task<BookDto> GetByIdAsync(int userId, int bookId)
    {
        var book = await FindOwnedAsync(userId, bookId);
        return ToDto(book);
    }

    public async Task<BookDto> CreateAsync(int userId, BookUpsertRequest request)
    {
        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            PublishedDate = request.PublishedDate,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        db.Books.Add(book);
        await db.SaveChangesAsync();

        return ToDto(book);
    }

    public async Task<BookDto> UpdateAsync(int userId, int bookId, BookUpsertRequest request)
    {
        var book = await FindOwnedAsync(userId, bookId);

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.PublishedDate = request.PublishedDate;

        await db.SaveChangesAsync();

        return ToDto(book);
    }

    public async Task DeleteAsync(int userId, int bookId)
    {
        var book = await FindOwnedAsync(userId, bookId);

        db.Books.Remove(book);
        await db.SaveChangesAsync();
    }

    // Både "finns inte" och "tillhör någon annan" ger samma NotFoundException,
    // så att en anropare inte kan avgöra om ett id existerar men ägs av någon annan.
    private async Task<Book> FindOwnedAsync(int userId, int bookId)
    {
        var book = await db.Books.FirstOrDefaultAsync(b => b.Id == bookId);

        if (book is null || book.UserId != userId)
        {
            throw new NotFoundException("Boken hittades inte.");
        }

        return book;
    }

    private static BookDto ToDto(Book book) => new(book.Id, book.Title, book.Author, book.PublishedDate);
}
