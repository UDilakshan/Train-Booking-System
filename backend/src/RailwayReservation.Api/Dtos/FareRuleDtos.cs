using System.ComponentModel.DataAnnotations;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateFareRuleRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = default!;

    public CoachType? CoachType { get; set; }

    [Required]
    public FareRuleType RuleType { get; set; }

    [Required]
    public FareValueType ValueType { get; set; }

    [Required]
    public decimal Value { get; set; }

    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public sealed class UpdateFareRuleRequest
{
    [MaxLength(120)]
    public string? Name { get; set; }

    public CoachType? CoachType { get; set; }
    public FareRuleType? RuleType { get; set; }
    public FareValueType? ValueType { get; set; }
    public decimal? Value { get; set; }
    public int? Priority { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
