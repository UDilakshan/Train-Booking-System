using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Name { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Staff;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
