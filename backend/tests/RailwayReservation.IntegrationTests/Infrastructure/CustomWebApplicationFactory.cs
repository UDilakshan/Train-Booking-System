using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the full app against a real MySQL instance — connection string comes from the
/// TEST_CONNECTION_STRING env var (defaults to the docker-compose `mysql` service's
/// credentials), so these tests exercise real transactions, row locks, and the
/// UNIQUE(seat_id, journey_id, leg_order) constraint (that's the whole point). NOT part of
/// `dotnet test` for the Api/unit projects — run explicitly via `dotnet test
/// tests/RailwayReservation.IntegrationTests` once the database is up and migrated.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
        ?? "Server=localhost;Port=3306;Database=railway_reservation;User=railway;Password=railway;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = ConnectionString,
                ["Jwt:Secret"] = "integration-test-secret-not-for-production-use-only",
                ["Jwt:Issuer"] = "railway-reservation",
            });
        });
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
