using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RailwayReservation.Application.Admin.Ports;
using RailwayReservation.Application.Auth.Ports;
using RailwayReservation.Application.Availability.Ports;
using RailwayReservation.Application.Bookings.Ports;
using RailwayReservation.Application.Common.Ports;
using RailwayReservation.Application.Fare.Ports;
using RailwayReservation.Infrastructure.Admin;
using RailwayReservation.Infrastructure.Auth;
using RailwayReservation.Infrastructure.Persistence;
using RailwayReservation.Infrastructure.Repositories;

namespace RailwayReservation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        // A fixed ServerVersion (rather than ServerVersion.AutoDetect) avoids a live DB
        // round-trip at startup/design-time — needed for `dotnet ef migrations add` to work
        // without a running database, and faster on every real boot too.
        services.AddDbContext<AppDbContext>(options => options
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 35)))
            .UseSnakeCaseNamingConvention());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
        services.AddScoped<IFareRuleRepository, FareRuleRepository>();
        services.AddScoped<IReferenceDataReader, ReferenceDataReader>();

        services.AddScoped<IUserReader, UserReader>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAdminReportingService, AdminReportingService>();

        return services;
    }
}
