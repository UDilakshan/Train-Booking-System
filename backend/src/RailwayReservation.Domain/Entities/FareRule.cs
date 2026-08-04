using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class FareRule
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public CoachType? CoachType { get; set; }
    public FareRuleType RuleType { get; set; }
    public FareValueType ValueType { get; set; }
    public decimal Value { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
