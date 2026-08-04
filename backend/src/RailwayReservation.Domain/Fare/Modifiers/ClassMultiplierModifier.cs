using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare.Modifiers;

/// <summary>Optional class-specific multiplier on top of the base fare (e.g. observation-saloon uplift). No-op if unconfigured.</summary>
public sealed class ClassMultiplierModifier : IFareModifier
{
    public decimal Apply(FareContext context, decimal runningFare, IReadOnlyList<FareRule> rules)
    {
        var rule = FareRuleSelector.SelectApplicableRule(rules, FareRuleType.ClassMultiplier, context.CoachType);
        return rule is null ? runningFare : FareRuleSelector.ApplyRuleValue(rule.Value, rule.ValueType, runningFare);
    }
}
