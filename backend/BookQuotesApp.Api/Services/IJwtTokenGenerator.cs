using BookQuotesApp.Api.Entities;

namespace BookQuotesApp.Api.Services;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
