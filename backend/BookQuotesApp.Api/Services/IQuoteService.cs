using BookQuotesApp.Api.Dtos.Quotes;

namespace BookQuotesApp.Api.Services;

public interface IQuoteService
{
    Task<IReadOnlyList<QuoteDto>> GetAllAsync(int userId);
    Task<QuoteDto> GetByIdAsync(int userId, int quoteId);
    Task<QuoteDto> CreateAsync(int userId, QuoteUpsertRequest request);
    Task<QuoteDto> UpdateAsync(int userId, int quoteId, QuoteUpsertRequest request);
    Task DeleteAsync(int userId, int quoteId);
}
