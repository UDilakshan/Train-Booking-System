using Microsoft.Extensions.DependencyInjection;
using RailwayReservation.Application.Auth;
using RailwayReservation.Application.Availability;
using RailwayReservation.Application.Bookings.UseCases;
using RailwayReservation.Application.Fare;

namespace RailwayReservation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<FareCalculationService>();
        services.AddScoped<GetFareQuoteUseCase>();
        services.AddScoped<GetAvailabilityUseCase>();

        services.AddScoped<CreateBookingUseCase>();
        services.AddScoped<GetBookingUseCase>();
        services.AddScoped<CancelBookingUseCase>();
        services.AddScoped<UpdateBookingUseCase>();

        services.AddScoped<LoginUseCase>();

        return services;
    }
}
