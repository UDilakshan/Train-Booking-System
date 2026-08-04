using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class Coach
{
    public Guid Id { get; set; }
    public Guid TrainId { get; set; }
    public string CoachNumber { get; set; } = default!;
    public CoachType CoachType { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Train Train { get; set; } = default!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
