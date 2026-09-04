using BookQuotesApp.Api.Dtos.Books;
using BookQuotesApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookQuotesApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController(IBookService bookService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> GetAll()
        => Ok(await bookService.GetAllAsync(currentUser.UserId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
        => Ok(await bookService.GetByIdAsync(currentUser.UserId, id));

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(BookUpsertRequest request)
    {
        var book = await bookService.CreateAsync(currentUser.UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BookDto>> Update(int id, BookUpsertRequest request)
        => Ok(await bookService.UpdateAsync(currentUser.UserId, id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await bookService.DeleteAsync(currentUser.UserId, id);
        return NoContent();
    }
}
