using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare.Modifiers;

/// <summary>Surcharge applied when the journey departs within a configured peak window (see FarePolicy).</summary>
public sealed class PeakSurchargeModifier : IFareModifier
{
    public decimal Apply(FareContext context, decimal runningFare, IReadOnlyList<FareRule> rules)
    {
        if (!FarePolicy.IsPeakDepartureTime(context.DepartureTime)) return runningFare;
        var rule = FareRuleSelector.SelectApplicableRule(rules, FareRuleType.Peak, context.CoachType);
        return rule is null ? runningFare : FareRuleSelector.ApplyRuleValue(rule.Value, rule.ValueType, runningFare);
    }
}
