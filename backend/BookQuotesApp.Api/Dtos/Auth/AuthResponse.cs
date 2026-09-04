namespace BookQuotesApp.Api.Dtos.Auth;

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email);
