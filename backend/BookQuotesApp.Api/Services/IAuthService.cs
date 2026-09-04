using BookQuotesApp.Api.Dtos.Auth;

namespace BookQuotesApp.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
