using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Fare.Ports;

public interface IFareRuleRepository
{
    /// <summary>All active fare rules whose effective window covers <paramref name="asOf"/> (or has no window set).</summary>
    Task<IReadOnlyList<FareRule>> GetApplicableRulesAsync(DateTime asOf, CancellationToken ct = default);
}
