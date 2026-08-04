using Microsoft.EntityFrameworkCore;
using RailwayReservation.Application.Admin.Dtos;
using RailwayReservation.Application.Admin.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Admin;

public sealed class AdminReportingService(AppDbContext db) : IAdminReportingService
{
    public async Task<OccupancyReport> GetOccupancyAsync(Guid journeyId, CancellationToken ct = default)
    {
        var journey = await db.Journeys.FirstOrDefaultAsync(j => j.Id == journeyId, ct)
            ?? throw new NotFoundAppException("JOURNEY_NOT_FOUND", $"Journey {journeyId} was not found.");

        var stations = await db.Stations.OrderBy(s => s.Order).ToListAsync(ct);
        var totalSeats = await db.Seats.CountAsync(s => s.Coach.TrainId == journey.TrainId, ct);

        // Pulled to memory: bounded by (seats booked x segments) for one journey, small dataset.
        var occupiedLegs = await db.BookingSegmentLegs
            .Where(l => l.JourneyId == journeyId)
            .Select(l => new { l.LegOrder, l.SeatId })
            .ToListAsync(ct);

        var legs = new List<SegmentLegUtilization>();
        for (var i = 0; i < stations.Count - 1; i++)
        {
            var from = stations[i];
            var to = stations[i + 1];
            var booked = occupiedLegs.Where(l => l.LegOrder >= from.Order && l.LegOrder < to.Order).Select(l => l.SeatId).Distinct().Count();
            legs.Add(new SegmentLegUtilization(from.Name, to.Name, booked, totalSeats, PercentOf(booked, totalSeats)));
        }

        var overall = legs.Count > 0 ? Round(legs.Average(l => l.UtilizationPercent)) : 0m;
        return new OccupancyReport(journeyId, totalSeats, overall, legs);
    }

    public async Task<IReadOnlyList<CoachUtilization>> GetCoachUtilizationAsync(Guid journeyId, CancellationToken ct = default)
    {
        var journey = await db.Journeys.FirstOrDefaultAsync(j => j.Id == journeyId, ct)
            ?? throw new NotFoundAppException("JOURNEY_NOT_FOUND", $"Journey {journeyId} was not found.");

        var coaches = await db.Coaches
            .Where(c => c.TrainId == journey.TrainId)
            .OrderBy(c => c.Order)
            .Select(c => new { c.Id, c.CoachNumber, c.CoachType, SeatIds = c.Seats.Select(s => s.Id).ToList() })
            .ToListAsync(ct);

        var bookedSeatIds = (await db.BookingSegmentLegs
            .Where(l => l.JourneyId == journeyId)
            .Select(l => l.SeatId)
            .Distinct()
            .ToListAsync(ct))
            .ToHashSet();

        return coaches.Select(c =>
        {
            var total = c.SeatIds.Count;
            var booked = c.SeatIds.Count(bookedSeatIds.Contains);
            return new CoachUtilization(c.Id, c.CoachNumber, c.CoachType, total, booked, total - booked, PercentOf(booked, total));
        }).ToList();
    }

    public async Task<RevenueReport> GetRevenueAsync(RevenueQuery query, CancellationToken ct = default)
    {
        var bookings = db.Bookings.Where(b => b.Status == BookingStatus.Confirmed);

        if (query.TrainId is { } trainId) bookings = bookings.Where(b => b.Journey.TrainId == trainId);
        if (query.From is { } from) bookings = bookings.Where(b => b.Journey.JourneyDate >= DateOnly.FromDateTime(from));
        if (query.To is { } to) bookings = bookings.Where(b => b.Journey.JourneyDate <= DateOnly.FromDateTime(to));

        var rows = await bookings
            .Select(b => new { b.TotalFare, b.Journey.TrainId, TrainName = b.Journey.Train.Name })
            .ToListAsync(ct);

        var byTrain = rows
            .GroupBy(r => new { r.TrainId, r.TrainName })
            .Select(g => new RevenueByTrain(g.Key.TrainId, g.Key.TrainName, Round(g.Sum(x => x.TotalFare)), g.Count()))
            .ToList();

        return new RevenueReport(Round(rows.Sum(r => r.TotalFare)), rows.Count, byTrain);
    }

    public async Task<JourneyStats> GetJourneyStatsAsync(Guid journeyId, CancellationToken ct = default)
    {
        var occupancy = await GetOccupancyAsync(journeyId, ct);
        var coachUtilization = await GetCoachUtilizationAsync(journeyId, ct);
        var confirmed = await db.Bookings.CountAsync(b => b.JourneyId == journeyId && b.Status == BookingStatus.Confirmed, ct);
        var cancelled = await db.Bookings.CountAsync(b => b.JourneyId == journeyId && b.Status == BookingStatus.Cancelled, ct);

        return new JourneyStats(journeyId, occupancy.TotalSeats, occupancy.OverallOccupancyPercent, confirmed, cancelled, occupancy.Legs, coachUtilization);
    }

    public async Task<IReadOnlyList<Booking>> ListBookingsAsync(ListBookingsQuery query, CancellationToken ct = default)
    {
        var bookings = db.Bookings
            .Include(b => b.Segments).ThenInclude(s => s.Seat).ThenInclude(seat => seat.Coach)
            .Include(b => b.OriginStation)
            .Include(b => b.DestinationStation)
            .Include(b => b.Journey).ThenInclude(j => j.Train)
            .AsSplitQuery()
            .AsQueryable();

        if (query.JourneyId is { } journeyId) bookings = bookings.Where(b => b.JourneyId == journeyId);
        if (query.Status is { } status) bookings = bookings.Where(b => b.Status == status);

        return await bookings.OrderByDescending(b => b.CreatedAt).Take(200).ToListAsync(ct);
    }

    private static decimal PercentOf(int part, int total) => total == 0 ? 0m : Round(part * 100m / total);

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
