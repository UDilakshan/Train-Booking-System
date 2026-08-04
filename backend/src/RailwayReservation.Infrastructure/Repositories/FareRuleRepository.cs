using Microsoft.EntityFrameworkCore;
using RailwayReservation.Application.Fare.Ports;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Repositories;

public sealed class FareRuleRepository(AppDbContext db) : IFareRuleRepository
{
    public async Task<IReadOnlyList<FareRule>> GetApplicableRulesAsync(DateTime asOf, CancellationToken ct = default) =>
        await db.FareRules
            .Where(r => r.IsActive
                && (r.EffectiveFrom == null || r.EffectiveFrom <= asOf)
                && (r.EffectiveTo == null || r.EffectiveTo >= asOf))
            .ToListAsync(ct);
}
