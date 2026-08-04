using Microsoft.EntityFrameworkCore;
using RailwayReservation.Application.Common.Ports;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Repositories;

public sealed class ReferenceDataReader(AppDbContext db) : IReferenceDataReader
{
    public Task<Station?> GetStationAsync(Guid id, CancellationToken ct = default) =>
        db.Stations.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Journey?> GetJourneyWithTrainAsync(Guid id, CancellationToken ct = default) =>
        db.Journeys.Include(j => j.Train).FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<IReadOnlyList<Seat>> GetSeatsWithCoachAsync(IReadOnlyCollection<Guid> seatIds, CancellationToken ct = default) =>
        await db.Seats.Include(s => s.Coach).Where(s => seatIds.Contains(s.Id)).ToListAsync(ct);
}
