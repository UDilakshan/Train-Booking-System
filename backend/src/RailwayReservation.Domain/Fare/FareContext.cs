using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare;

/// <summary>Everything a fare modifier needs to price one seat-segment. Extend this, not modifier signatures, for new inputs.</summary>
public sealed record FareContext(decimal DistanceKm, CoachType CoachType, string DepartureTime, bool IsExpress, DateTime JourneyDate);
