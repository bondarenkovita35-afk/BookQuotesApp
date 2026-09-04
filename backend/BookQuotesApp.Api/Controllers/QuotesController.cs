using BookQuotesApp.Api.Dtos.Quotes;
using BookQuotesApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookQuotesApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotesController(IQuoteService quoteService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuoteDto>>> GetAll()
        => Ok(await quoteService.GetAllAsync(currentUser.UserId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuoteDto>> GetById(int id)
        => Ok(await quoteService.GetByIdAsync(currentUser.UserId, id));

    [HttpPost]
    public async Task<ActionResult<QuoteDto>> Create(QuoteUpsertRequest request)
    {
        var quote = await quoteService.CreateAsync(currentUser.UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = quote.Id }, quote);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<QuoteDto>> Update(int id, QuoteUpsertRequest request)
        => Ok(await quoteService.UpdateAsync(currentUser.UserId, id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await quoteService.DeleteAsync(currentUser.UserId, id);
        return NoContent();
    }
}
