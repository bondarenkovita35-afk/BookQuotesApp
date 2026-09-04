using System.Security.Claims;

namespace BookQuotesApp.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (value is null || !int.TryParse(value, out var userId))
            {
                throw new InvalidOperationException("Ingen inloggad användare hittades i kontexten.");
            }

            return userId;
        }
    }
}
