using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Admin.Dtos;

public sealed record SegmentLegUtilization(string FromStation, string ToStation, int BookedSeats, int TotalSeats, decimal UtilizationPercent);

public sealed record OccupancyReport(Guid JourneyId, int TotalSeats, decimal OverallOccupancyPercent, IReadOnlyList<SegmentLegUtilization> Legs);

public sealed record CoachUtilization(Guid CoachId, string CoachNumber, CoachType CoachType, int TotalSeats, int BookedSeats, int AvailableSeats, decimal UtilizationPercent);

public sealed record RevenueByTrain(Guid TrainId, string TrainName, decimal Revenue, int BookingsCount);

public sealed record RevenueReport(decimal TotalRevenue, int BookingsCount, IReadOnlyList<RevenueByTrain> ByTrain);

public sealed record JourneyStats(
    Guid JourneyId,
    int TotalSeats,
    decimal OverallOccupancyPercent,
    int ConfirmedBookings,
    int CancelledBookings,
    IReadOnlyList<SegmentLegUtilization> SegmentUtilization,
    IReadOnlyList<CoachUtilization> CoachUtilization);

public sealed record RevenueQuery(DateTime? From, DateTime? To, Guid? TrainId);

public sealed record ListBookingsQuery(Guid? JourneyId, BookingStatus? Status);
