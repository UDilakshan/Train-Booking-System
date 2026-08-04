using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Fare.Modifiers;

/// <summary>
/// Applies the highest-priority active DISCOUNT rule, if any. No discount rules are seeded by
/// default, so this is a no-op today — it exists so student/season-ticket/promotional discounts
/// can be turned on later purely via FareRule data (through /admin/fare-rules), with no code change.
/// </summary>
public sealed class DiscountModifier : IFareModifier
{
    public decimal Apply(FareContext context, decimal runningFare, IReadOnlyList<FareRule> rules)
    {
        var rule = FareRuleSelector.SelectApplicableRule(rules, FareRuleType.Discount, context.CoachType);
        if (rule is null) return runningFare;

        // Discount values are stored positive; subtract rather than add.
        var negativeValue = -Math.Abs(rule.Value);
        return FareRuleSelector.ApplyRuleValue(negativeValue, rule.ValueType, runningFare);
    }
}
