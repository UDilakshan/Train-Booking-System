using System.ComponentModel.DataAnnotations;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateJourneyRequest
{
    [Required]
    public Guid TrainId { get; set; }

    [Required]
    public DateOnly JourneyDate { get; set; }

    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Expected HH:MM 24h format")]
    public string DepartureTime { get; set; } = default!;

    public JourneyStatus Status { get; set; } = JourneyStatus.Scheduled;
}

public sealed class UpdateJourneyRequest
{
    public DateOnly? JourneyDate { get; set; }

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Expected HH:MM 24h format")]
    public string? DepartureTime { get; set; }

    public JourneyStatus? Status { get; set; }
}
