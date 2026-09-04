using System.Net;
using System.Net.Http.Json;
using BookQuotesApp.Api.Dtos.Books;

namespace BookQuotesApp.Tests.Integration;

public class BooksTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task CreateBook_ThenGetById_ReturnsSameBook()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var created = await CreateBookAsync(client);
        var getResponse = await client.GetAsync($"/api/books/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.Equal(created.Title, fetched!.Title);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnBooks()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        await CreateBookAsync(client);
        await CreateBookAsync(client);

        var response = await client.GetAsync("/api/books");

        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.Equal(2, books!.Count);
    }

    [Fact]
    public async Task CreateBook_WithEmptyTitle_ReturnsBadRequest()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/books",
            new { title = "", author = "", publishedDate = "1954-07-29" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_ChangesFields()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateBookAsync(client);

        var updateResponse = await client.PutAsJsonAsync($"/api/books/{created.Id}", new
        {
            title = "Uppdaterad titel",
            author = created.Author,
            publishedDate = created.PublishedDate
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.Equal("Uppdaterad titel", updated!.Title);
    }

    [Fact]
    public async Task DeleteBook_RemovesIt()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateBookAsync(client);

        var deleteResponse = await client.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task User_CannotReadAnotherUsersBook()
    {
        var ownerClient = await factory.CreateAuthenticatedClientAsync();
        var book = await CreateBookAsync(ownerClient);

        var otherClient = await factory.CreateAuthenticatedClientAsync();
        var response = await otherClient.GetAsync($"/api/books/{book.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task User_CannotDeleteAnotherUsersBook_AndItSurvives()
    {
        var ownerClient = await factory.CreateAuthenticatedClientAsync();
        var book = await CreateBookAsync(ownerClient);

        var otherClient = await factory.CreateAuthenticatedClientAsync();
        var deleteResponse = await otherClient.DeleteAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var stillThereResponse = await ownerClient.GetAsync($"/api/books/{book.Id}");
        Assert.Equal(HttpStatusCode.OK, stillThereResponse.StatusCode);
    }

    [Fact]
    public async Task User_CannotUpdateAnotherUsersBook()
    {
        var ownerClient = await factory.CreateAuthenticatedClientAsync();
        var book = await CreateBookAsync(ownerClient);

        var otherClient = await factory.CreateAuthenticatedClientAsync();
        var response = await otherClient.PutAsJsonAsync($"/api/books/{book.Id}", new
        {
            title = "Kapad titel",
            author = book.Author,
            publishedDate = book.PublishedDate
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<BookDto> CreateBookAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/books", new
        {
            title = "Sagan om ringen",
            author = "J.R.R. Tolkien",
            publishedDate = "1954-07-29"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BookDto>())!;
    }
}
