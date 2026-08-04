using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayReservation.Application.Admin.Dtos;
using RailwayReservation.Application.Admin.Ports;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("admin"), Authorize]
public class AdminController(IAdminReportingService adminReportingService) : ControllerBase
{
    [HttpGet("occupancy")]
    public Task<OccupancyReport> Occupancy([FromQuery] Guid journeyId) => adminReportingService.GetOccupancyAsync(journeyId);

    [HttpGet("segment-utilization")]
    public async Task<IReadOnlyList<SegmentLegUtilization>> SegmentUtilization([FromQuery] Guid journeyId) =>
        (await adminReportingService.GetOccupancyAsync(journeyId)).Legs;

    [HttpGet("coach-utilization")]
    public Task<IReadOnlyList<CoachUtilization>> CoachUtilization([FromQuery] Guid journeyId) => adminReportingService.GetCoachUtilizationAsync(journeyId);

    [HttpGet("revenue")]
    public Task<RevenueReport> Revenue([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Guid? trainId) =>
        adminReportingService.GetRevenueAsync(new RevenueQuery(from, to, trainId));

    [HttpGet("journeys/stats")]
    public Task<JourneyStats> JourneyStats([FromQuery] Guid journeyId) => adminReportingService.GetJourneyStatsAsync(journeyId);

    [HttpGet("bookings")]
    public Task<IReadOnlyList<Booking>> Bookings([FromQuery] Guid? journeyId, [FromQuery] BookingStatus? status) =>
        adminReportingService.ListBookingsAsync(new ListBookingsQuery(journeyId, status));
}
