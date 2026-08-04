using System.ComponentModel.DataAnnotations;

namespace RailwayReservation.Api.Dtos;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;
}
