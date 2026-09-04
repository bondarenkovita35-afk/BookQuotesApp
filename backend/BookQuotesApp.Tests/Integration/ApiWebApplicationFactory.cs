using BookQuotesApp.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookQuotesApp.Tests.Integration;

/// <summary>
/// Kör API:et mot en SQLite-databas i minnet istället för SQL Server,
/// så att testerna inte kräver LocalDB och alltid startar tomma.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "BookQuotesApp.Tests",
                ["Jwt:Audience"] = "BookQuotesApp.Tests",
                ["Jwt:SigningKey"] = "test-only-signing-key-not-used-anywhere-real-1234567890",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Cors:AllowedOrigins:0"] = "http://localhost:4200"
            });
        });

        builder.ConfigureServices(services =>
        {
            // AddDbContext lagrar sin konfiguration (UseSqlServer) i en separat
            // IDbContextOptionsConfiguration-post som inte tas bort av att bara
            // ta bort DbContextOptions<AppDbContext> — annars klagar EF Core på
            // att två providers (SqlServer och Sqlite) är registrerade samtidigt.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            _connection.Open();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
