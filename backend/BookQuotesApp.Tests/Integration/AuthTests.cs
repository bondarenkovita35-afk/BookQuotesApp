using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookQuotesApp.Api.Dtos.Auth;
using BookQuotesApp.Api.Dtos.Quotes;

namespace BookQuotesApp.Tests.Integration;

public class AuthTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithNewEmail_ReturnsCreatedWithToken()
    {
        var email = TestHelpers.UniqueEmail();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(email, body.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = TestHelpers.UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123" });

        var response = await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithTooShortPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = TestHelpers.UniqueEmail(), password = "short" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_CreatesExactlyFiveStarterQuotes()
    {
        var token = await TestHelpers.RegisterAndGetTokenAsync(_client);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/quotes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        var quotes = await response.Content.ReadFromJsonAsync<List<QuoteDto>>();
        Assert.Equal(5, quotes!.Count);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsOk()
    {
        var email = TestHelpers.UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = TestHelpers.UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = TestHelpers.UniqueEmail(), password = "Password123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid-jwt");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
