using System.ComponentModel.DataAnnotations;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateCoachRequest
{
    [Required]
    public Guid TrainId { get; set; }

    [Required, MaxLength(10)]
    public string CoachNumber { get; set; } = default!;

    [Required]
    public CoachType CoachType { get; set; }

    [Range(0, int.MaxValue)]
    public int Order { get; set; }

    /// <summary>Convenience: auto-generate this many seats numbered 01..N when creating the coach.</summary>
    [Range(1, 200)]
    public int? SeatCount { get; set; }
}

public sealed class UpdateCoachRequest
{
    [MaxLength(10)]
    public string? CoachNumber { get; set; }

    public CoachType? CoachType { get; set; }
    public int? Order { get; set; }
}
