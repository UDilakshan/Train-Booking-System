using RailwayReservation.Application.Bookings.Dtos;
using RailwayReservation.Application.Bookings.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Bookings.UseCases;

/// <summary>Only passenger contact details are editable post-booking — the journey/seats/segment are immutable once confirmed (cancel + rebook instead).</summary>
public sealed class UpdateBookingUseCase(IBookingRepository bookingRepository)
{
    public async Task<Booking> ExecuteAsync(string reference, UpdateBookingRequest request, CancellationToken ct = default)
    {
        var booking = await bookingRepository.FindByReferenceAsync(reference, ct)
            ?? throw new NotFoundAppException("BOOKING_NOT_FOUND", $"Booking {reference} was not found.");

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictAppException("BOOKING_CANCELLED", "Cannot update a cancelled booking.");
        }

        return await bookingRepository.UpdatePassengerDetailsAsync(booking.Id, request.PassengerName, request.PassengerContact, ct);
    }
}
