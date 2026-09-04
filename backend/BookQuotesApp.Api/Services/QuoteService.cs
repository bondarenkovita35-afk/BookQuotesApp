using BookQuotesApp.Api.Common;
using BookQuotesApp.Api.Data;
using BookQuotesApp.Api.Dtos.Quotes;
using BookQuotesApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookQuotesApp.Api.Services;

public class QuoteService(AppDbContext db) : IQuoteService
{
    public async Task<IReadOnlyList<QuoteDto>> GetAllAsync(int userId)
    {
        return await db.Quotes
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => ToDto(q))
            .ToListAsync();
    }

    public async Task<QuoteDto> GetByIdAsync(int userId, int quoteId)
    {
        var quote = await FindOwnedAsync(userId, quoteId);
        return ToDto(quote);
    }

    public async Task<QuoteDto> CreateAsync(int userId, QuoteUpsertRequest request)
    {
        var quote = new Quote
        {
            Text = request.Text.Trim(),
            Author = string.IsNullOrWhiteSpace(request.Author) ? null : request.Author.Trim(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        db.Quotes.Add(quote);
        await db.SaveChangesAsync();

        return ToDto(quote);
    }

    public async Task<QuoteDto> UpdateAsync(int userId, int quoteId, QuoteUpsertRequest request)
    {
        var quote = await FindOwnedAsync(userId, quoteId);

        quote.Text = request.Text.Trim();
        quote.Author = string.IsNullOrWhiteSpace(request.Author) ? null : request.Author.Trim();

        await db.SaveChangesAsync();

        return ToDto(quote);
    }

    public async Task DeleteAsync(int userId, int quoteId)
    {
        var quote = await FindOwnedAsync(userId, quoteId);

        db.Quotes.Remove(quote);
        await db.SaveChangesAsync();
    }

    // Samma resonemang som i BookService: "finns inte" och "tillhör någon annan"
    // ska se likadana ut utåt.
    private async Task<Quote> FindOwnedAsync(int userId, int quoteId)
    {
        var quote = await db.Quotes.FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote is null || quote.UserId != userId)
        {
            throw new NotFoundException("Citatet hittades inte.");
        }

        return quote;
    }

    private static QuoteDto ToDto(Quote quote) => new(quote.Id, quote.Text, quote.Author);
}
