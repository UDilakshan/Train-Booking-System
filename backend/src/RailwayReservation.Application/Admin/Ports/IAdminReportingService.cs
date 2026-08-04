using RailwayReservation.Application.Admin.Dtos;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Admin.Ports;

/// <summary>
/// Read-only reporting/aggregation queries for the admin dashboard. Implemented directly against
/// the DbContext in Infrastructure — like the reference-data CRUD modules, these are plain
/// aggregation queries with no business invariants of their own, so they skip a use-case layer
/// (see README "Clean Architecture scoping").
/// </summary>
public interface IAdminReportingService
{
    Task<OccupancyReport> GetOccupancyAsync(Guid journeyId, CancellationToken ct = default);
    Task<IReadOnlyList<CoachUtilization>> GetCoachUtilizationAsync(Guid journeyId, CancellationToken ct = default);
    Task<RevenueReport> GetRevenueAsync(RevenueQuery query, CancellationToken ct = default);
    Task<JourneyStats> GetJourneyStatsAsync(Guid journeyId, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> ListBookingsAsync(ListBookingsQuery query, CancellationToken ct = default);
}
