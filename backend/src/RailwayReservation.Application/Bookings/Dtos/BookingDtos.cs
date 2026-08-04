using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Bookings.Dtos;

public sealed record CreateBookingRequest(
    Guid JourneyId,
    Guid OriginStationId,
    Guid DestinationStationId,
    string PassengerName,
    string PassengerContact,
    IReadOnlyList<Guid> SeatIds);

public sealed record UpdateBookingRequest(string? PassengerName, string? PassengerContact);

public sealed record BookingSegmentDto(
    Guid Id,
    Guid SeatId,
    string SeatNumber,
    Guid CoachId,
    string CoachNumber,
    CoachType CoachType,
    int OriginOrder,
    int DestinationOrder,
    decimal Fare,
    BookingStatus Status);

public sealed record BookingDto(
    Guid Id,
    string BookingReference,
    Guid JourneyId,
    DateOnly JourneyDate,
    string DepartureTime,
    string TrainName,
    Guid OriginStationId,
    string OriginStationName,
    Guid DestinationStationId,
    string DestinationStationName,
    string PassengerName,
    string PassengerContact,
    decimal TotalFare,
    BookingStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<BookingSegmentDto> Segments);
