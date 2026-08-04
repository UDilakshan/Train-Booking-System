using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare.Modifiers;

/// <summary>Surcharge for express/intercity services (Train.IsExpress), e.g. Podi Menike.</summary>
public sealed class ExpressSurchargeModifier : IFareModifier
{
    public decimal Apply(FareContext context, decimal runningFare, IReadOnlyList<FareRule> rules)
    {
        if (!context.IsExpress) return runningFare;
        var rule = FareRuleSelector.SelectApplicableRule(rules, FareRuleType.Express, context.CoachType);
        return rule is null ? runningFare : FareRuleSelector.ApplyRuleValue(rule.Value, rule.ValueType, runningFare);
    }
}
