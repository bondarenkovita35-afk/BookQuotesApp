using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookQuotesApp.Api.Dtos.Auth;

namespace BookQuotesApp.Tests.Integration;

public static class TestHelpers
{
    public static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";

    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client, string? email = null)
    {
        email ??= UniqueEmail();

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password = "Password123" });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return body!.Token;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this ApiWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var token = await RegisterAndGetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
