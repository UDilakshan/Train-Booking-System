using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Bookings.Ports;

public sealed record CreateBookingSegmentCommand(Guid SeatId, decimal Fare);

public sealed record CreateBookingCommand(
    Guid JourneyId,
    Guid OriginStationId,
    Guid DestinationStationId,
    int OriginOrder,
    int DestinationOrder,
    string PassengerName,
    string PassengerContact,
    string BookingReference,
    decimal TotalFare,
    IReadOnlyList<CreateBookingSegmentCommand> Segments);

public interface IBookingRepository
{
    /// <summary>Runs the full concurrency-safe create (row lock + overlap check + insert) — see infrastructure implementation for the strategy writeup.</summary>
    Task<Booking> CreateAsync(CreateBookingCommand command, CancellationToken ct = default);

    Task<Booking?> FindByReferenceAsync(string reference, CancellationToken ct = default);

    Task<Booking?> FindByIdAsync(Guid id, CancellationToken ct = default);

    Task<Booking> CancelAsync(Guid id, CancellationToken ct = default);

    Task<Booking> UpdatePassengerDetailsAsync(Guid id, string? passengerName, string? passengerContact, CancellationToken ct = default);
}
