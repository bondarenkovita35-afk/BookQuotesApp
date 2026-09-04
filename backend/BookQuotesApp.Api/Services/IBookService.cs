using BookQuotesApp.Api.Dtos.Books;

namespace BookQuotesApp.Api.Services;

public interface IBookService
{
    Task<IReadOnlyList<BookDto>> GetAllAsync(int userId);
    Task<BookDto> GetByIdAsync(int userId, int bookId);
    Task<BookDto> CreateAsync(int userId, BookUpsertRequest request);
    Task<BookDto> UpdateAsync(int userId, int bookId, BookUpsertRequest request);
    Task DeleteAsync(int userId, int bookId);
}
