using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare.Modifiers;

/// <summary>Distance x rate/km, using the most specific active BASE rule for the coach type (falls back to the wildcard rule).</summary>
public sealed class BaseDistanceFareModifier : IFareModifier
{
    public decimal Apply(FareContext context, decimal runningFare, IReadOnlyList<FareRule> rules)
    {
        var rule = FareRuleSelector.SelectApplicableRule(rules, FareRuleType.Base, context.CoachType);
        return rule is null ? runningFare : runningFare + rule.Value * context.DistanceKm;
    }
}
