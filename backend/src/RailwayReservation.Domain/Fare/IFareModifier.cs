using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Domain.Fare;

/// <summary>
/// One pricing step in the fare pipeline (base rate, class multiplier, peak surcharge, ...). New
/// pricing policies — student discounts, season tickets, dynamic demand pricing — are added by
/// implementing this interface and registering the modifier in FareCalculationService's
/// pipeline, without touching booking or any other modifier's logic.
/// </summary>
public interface IFareModifier
{
    decimal Apply(FareContext context, decimal runningFare, IReadOnlyList<FareRule> rules);
}
