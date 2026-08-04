using System.ComponentModel.DataAnnotations;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateWaitlistEntryRequest
{
    [Required]
    public Guid JourneyId { get; set; }

    [Required]
    public Guid OriginStationId { get; set; }

    [Required]
    public Guid DestinationStationId { get; set; }

    [Required, MinLength(2), MaxLength(120)]
    [RegularExpression(@"^[A-Za-z][A-Za-z .'-]*$", ErrorMessage = "Passenger name can only contain letters, spaces, apostrophes and hyphens")]
    public string PassengerName { get; set; } = default!;

    [Required]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be exactly 10 digits")]
    public string PassengerContact { get; set; } = default!;
}

public sealed class UpdateWaitlistEntryRequest
{
    [Required]
    public WaitlistStatus Status { get; set; }
}
