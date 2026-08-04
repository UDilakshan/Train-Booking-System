using System.ComponentModel.DataAnnotations;

namespace RailwayReservation.Api.Dtos;

public sealed class CreateSeatRequest
{
    [Required]
    public Guid CoachId { get; set; }

    [Required, MaxLength(10)]
    public string SeatNumber { get; set; } = default!;

    [MaxLength(20)]
    public string? SeatType { get; set; }
}

public sealed class UpdateSeatRequest
{
    [MaxLength(10)]
    public string? SeatNumber { get; set; }

    [MaxLength(20)]
    public string? SeatType { get; set; }
}
