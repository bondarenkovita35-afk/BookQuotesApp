using System.Text;
using BookQuotesApp.Api.Data;
using BookQuotesApp.Api.Dtos.Common;
using BookQuotesApp.Api.Entities;
using BookQuotesApp.Api.HealthChecks;
using BookQuotesApp.Api.Middleware;
using BookQuotesApp.Api.Options;
using BookQuotesApp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Konfigureras via IOptions<JwtOptions> (inte en variabel som läses in tidigt),
// så att bearer-valideringen och JwtTokenGenerator garanterat använder exakt
// samma nyckel, oavsett i vilken ordning konfigurationskällorna läggs på.
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
    {
        var jwt = jwtOptionsAccessor.Value;

        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Ger samma JSON-felformat på svenska även när token saknas, är ogiltig
        // eller har gått ut, istället för ASP.NET Cores tomma standardsvar.
        bearerOptions.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var message = context.AuthenticateFailure is SecurityTokenExpiredException
                    ? "Sessionen har gått ut. Logga in igen."
                    : "Du måste vara inloggad för att göra detta.";

                await context.Response.WriteAsJsonAsync(new ErrorResponse(message));
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BookQuotesApp API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Ange JWT-token: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

var app = builder.Build();

// Kontrolleras här (efter Build) och inte tidigare på builder.Configuration — annars
// läses värdet in innan WebApplicationFactory i testerna hunnit lägga på sin egen
// konfiguration, vilket fick kontrollen att slå till felaktigt i CI.
if (string.IsNullOrWhiteSpace(app.Configuration["Jwt:SigningKey"]))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey saknas. Sätt den via 'dotnet user-secrets' lokalt eller som miljövariabel i produktion.");
}

// Applicerar väntande migrationer automatiskt vid start. Enkelt och reproducerbart
// för en enda instans utan skalning — vid flera samtidiga instanser hade en separat
// migreringssteg i pipelinen varit säkrare för att undvika race conditions.
// Görs inte i "Testing" — testfabriken byter ut SQL Server mot SQLite och bygger
// sitt eget schema direkt, vilket krockar med SQL Server-migrationernas modellsnapshot.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var migrationScope = app.Services.CreateScope();
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Gör klassen synlig för WebApplicationFactory<Program> i integrationstester.
public partial class Program;
