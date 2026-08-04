using RailwayReservation.Application.Bookings.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Bookings.UseCases;

public sealed class GetBookingUseCase(IBookingRepository bookingRepository)
{
    public async Task<Booking> ExecuteAsync(string reference, CancellationToken ct = default) =>
        await bookingRepository.FindByReferenceAsync(reference, ct)
            ?? throw new NotFoundAppException("BOOKING_NOT_FOUND", $"Booking {reference} was not found.");
}
