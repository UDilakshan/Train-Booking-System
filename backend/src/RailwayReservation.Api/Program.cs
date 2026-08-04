using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using RailwayReservation.Api.Common;
using RailwayReservation.Api.Common.Filters;
using RailwayReservation.Api.Common.Middleware;
using RailwayReservation.Application;
using RailwayReservation.Infrastructure;
using RailwayReservation.Infrastructure.Auth;
using RailwayReservation.Infrastructure.Persistence;
using RailwayReservation.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ApiResponseWrappingFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Keep DataAnnotations validation on request DTOs, but shape the 400 the same as every
        // other error — the standard envelope, not ASP.NET Core's default ValidationProblemDetails.
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(kvp => kvp.Value?.Errors.Count > 0)
                .Select(kvp => new { path = kvp.Key, message = kvp.Value!.Errors.First().ErrorMessage });
            return new BadRequestObjectResult(new ApiErrorResponse(false, new ApiErrorBody("VALIDATION_ERROR", "Request validation failed.", details)));
        };
    });

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
var jwtIssuer = jwtSection["Issuer"] ?? "railway-reservation";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
    };
});
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    var origin = builder.Configuration["Cors:Origin"] ?? "http://localhost:4200";
    policy.WithOrigins(origin).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
    options.RejectionStatusCode = 429;
});

var app = builder.Build();

// `dotnet run -- seed` applies pending migrations then seeds reference data, then exits — the
// equivalent of the original build's `prisma migrate deploy && prisma db seed` step, run from
// docker-entrypoint.sh before the app starts serving traffic.
if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>());
    return;
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program;
