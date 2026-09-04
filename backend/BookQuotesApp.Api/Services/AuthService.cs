using BookQuotesApp.Api.Common;
using BookQuotesApp.Api.Data;
using BookQuotesApp.Api.Dtos.Auth;
using BookQuotesApp.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookQuotesApp.Api.Services;

public class AuthService(
    AppDbContext db,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenGenerator tokenGenerator) : IAuthService
{
    // Startciten som varje ny användare får. Läggs till i samma SaveChanges-anrop
    // som skapar användaren, så de kan aldrig skapas dubbelt eller separat.
    private static readonly string[] StarterQuoteTexts =
    [
        "Varje bok är en dörr till en ny värld.",
        "Det du lär dig idag bär du med dig imorgon.",
        "Små steg varje dag leder till stora förändringar.",
        "Nyfikenhet är början på all kunskap.",
        "Den bästa tiden att börja är nu."
    ];

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException("E-postadressen används redan.");
        }

        var user = new User
        {
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        foreach (var text in StarterQuoteTexts)
        {
            user.Quotes.Add(new Quote { Text = text, CreatedAt = DateTime.UtcNow });
        }

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Fel e-postadress eller lösenord.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Fel e-postadress eller lösenord.");
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user);
        return new AuthResponse(token, expiresAtUtc, user.Email);
    }
}
