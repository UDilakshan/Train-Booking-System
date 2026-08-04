using Microsoft.EntityFrameworkCore;
using RailwayReservation.Application.Availability.Ports;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Repositories;

public sealed class AvailabilityRepository(AppDbContext db) : IAvailabilityRepository
{
    public async Task<IReadOnlyList<SeatRow>> GetSeatsForJourneyTrainAsync(Guid journeyId, CancellationToken ct = default)
    {
        var trainId = await db.Journeys.Where(j => j.Id == journeyId).Select(j => j.TrainId).FirstOrDefaultAsync(ct);

        return await db.Seats
            .Where(s => s.Coach.TrainId == trainId)
            .OrderBy(s => s.Coach.Order).ThenBy(s => s.SeatNumber)
            .Select(s => new SeatRow(s.Id, s.SeatNumber, s.SeatType, s.CoachId, s.Coach.CoachNumber, s.Coach.CoachType, s.Coach.Order))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlySet<Guid>> GetOccupiedSeatIdsAsync(Guid journeyId, int originOrder, int destinationOrder, CancellationToken ct = default)
    {
        var legs = Enumerable.Range(originOrder, destinationOrder - originOrder).ToHashSet();

        var seatIds = await db.BookingSegmentLegs
            .Where(l => l.JourneyId == journeyId && legs.Contains(l.LegOrder))
            .Select(l => l.SeatId)
            .Distinct()
            .ToListAsync(ct);

        return seatIds.ToHashSet();
    }
}
