using BookQuotesApp.Api.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookQuotesApp.Api.HealthChecks;

public class DatabaseHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Kan inte ansluta till databasen.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Kan inte ansluta till databasen.", ex);
        }
    }
}
