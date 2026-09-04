using BookQuotesApp.Api.Common;
using BookQuotesApp.Api.Dtos.Common;

namespace BookQuotesApp.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, message) = ex switch
            {
                ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Ett oväntat fel har uppstått.")
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(ex, "Ohanterat fel vid {Path}", context.Request.Path);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ErrorResponse(message));
        }
    }
}
