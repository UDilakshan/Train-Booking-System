using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare;

public static class FareRuleSelector
{
    /// <summary>
    /// Picks the single most applicable rule of a given type: an exact coach-type match beats a
    /// wildcard (CoachType == null) rule, and among ties the higher Priority wins. Returns null
    /// if nothing matches.
    /// </summary>
    public static FareRule? SelectApplicableRule(IReadOnlyList<FareRule> rules, FareRuleType ruleType, CoachType coachType)
    {
        var candidates = rules
            .Where(r => r.RuleType == ruleType && (r.CoachType == null || r.CoachType == coachType))
            .ToList();

        if (candidates.Count == 0) return null;

        return candidates
            .OrderByDescending(r => r.CoachType != null)
            .ThenByDescending(r => r.Priority)
            .First();
    }

    public static decimal ApplyRuleValue(decimal ruleValue, FareValueType valueType, decimal runningFare) => valueType switch
    {
        FareValueType.Percent => runningFare + runningFare * (ruleValue / 100m),
        FareValueType.Flat => runningFare + ruleValue,
        FareValueType.Multiplier => runningFare * ruleValue,
        // PerKm is handled directly by BaseDistanceFareModifier, which has distanceKm in scope.
        FareValueType.PerKm => runningFare,
        _ => runningFare,
    };
}
