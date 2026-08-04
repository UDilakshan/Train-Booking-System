using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Availability.Dtos;

public sealed record SeatAvailabilityDto(
    Guid SeatId,
    string SeatNumber,
    string? SeatType,
    Guid CoachId,
    string CoachNumber,
    CoachType CoachType,
    int CoachOrder,
    bool IsAvailable);
