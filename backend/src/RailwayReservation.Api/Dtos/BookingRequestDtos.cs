using System.ComponentModel.DataAnnotations;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateBookingRequestDto
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

    [Required, MinLength(1, ErrorMessage = "Select at least one seat"), MaxLength(10, ErrorMessage = "Book at most 10 seats at once")]
    public List<Guid> SeatIds { get; set; } = [];
}

public sealed class UpdateBookingRequestDto
{
    [MinLength(2), MaxLength(120)]
    [RegularExpression(@"^[A-Za-z][A-Za-z .'-]*$", ErrorMessage = "Passenger name can only contain letters, spaces, apostrophes and hyphens")]
    public string? PassengerName { get; set; }

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact number must be exactly 10 digits")]
    public string? PassengerContact { get; set; }
}
