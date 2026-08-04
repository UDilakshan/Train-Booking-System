using RailwayReservation.Application.Bookings.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Bookings.UseCases;

public sealed class CancelBookingUseCase(IBookingRepository bookingRepository)
{
    public async Task<Booking> ExecuteAsync(string reference, CancellationToken ct = default)
    {
        var booking = await bookingRepository.FindByReferenceAsync(reference, ct)
            ?? throw new NotFoundAppException("BOOKING_NOT_FOUND", $"Booking {reference} was not found.");

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictAppException("ALREADY_CANCELLED", "This booking is already cancelled.");
        }

        return await bookingRepository.CancelAsync(booking.Id, ct);
    }
}
