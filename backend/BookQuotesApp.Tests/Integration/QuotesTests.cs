using System.Net;
using System.Net.Http.Json;
using BookQuotesApp.Api.Dtos.Quotes;

namespace BookQuotesApp.Tests.Integration;

public class QuotesTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    [Fact]
    public async Task NewUser_SeesFiveStarterQuotes_AndCanAddASixth()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        await CreateQuoteAsync(client);

        var response = await client.GetAsync("/api/quotes");
        var quotes = await response.Content.ReadFromJsonAsync<List<QuoteDto>>();

        Assert.Equal(6, quotes!.Count);
    }

    [Fact]
    public async Task CreateQuote_WithEmptyText_ReturnsBadRequest()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/quotes", new { text = "", author = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateQuote_ChangesText()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateQuoteAsync(client);

        var updateResponse = await client.PutAsJsonAsync($"/api/quotes/{created.Id}", new
        {
            text = "Ett uppdaterat citat.",
            author = "Ny författare"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuoteDto>();
        Assert.Equal("Ett uppdaterat citat.", updated!.Text);
    }

    [Fact]
    public async Task DeleteQuote_RemovesIt()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateQuoteAsync(client);

        var deleteResponse = await client.DeleteAsync($"/api/quotes/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/quotes/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task User_CannotAccessAnotherUsersQuote()
    {
        var ownerClient = await factory.CreateAuthenticatedClientAsync();
        var quote = await CreateQuoteAsync(ownerClient);

        var otherClient = await factory.CreateAuthenticatedClientAsync();

        var getResponse = await otherClient.GetAsync($"/api/quotes/{quote.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var deleteResponse = await otherClient.DeleteAsync($"/api/quotes/{quote.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);

        var stillThereResponse = await ownerClient.GetAsync($"/api/quotes/{quote.Id}");
        Assert.Equal(HttpStatusCode.OK, stillThereResponse.StatusCode);
    }

    [Fact]
    public async Task TwoNewUsers_GetSeparateSetsOfStarterQuotes()
    {
        var clientA = await factory.CreateAuthenticatedClientAsync();
        var clientB = await factory.CreateAuthenticatedClientAsync();

        var quotesA = await (await clientA.GetAsync("/api/quotes")).Content.ReadFromJsonAsync<List<QuoteDto>>();
        var quotesB = await (await clientB.GetAsync("/api/quotes")).Content.ReadFromJsonAsync<List<QuoteDto>>();

        var idsA = quotesA!.Select(q => q.Id).ToHashSet();
        var idsB = quotesB!.Select(q => q.Id).ToHashSet();

        Assert.Equal(5, idsA.Count);
        Assert.Equal(5, idsB.Count);
        Assert.Empty(idsA.Intersect(idsB));
    }

    private static async Task<QuoteDto> CreateQuoteAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/quotes", new
        {
            text = "Ett nytt citat för testet.",
            author = "Testförfattare"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuoteDto>())!;
    }
}
