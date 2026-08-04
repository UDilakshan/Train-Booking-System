namespace RailwayReservation.Domain.Entities;

public class Station
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Order { get; set; }
    public decimal DistanceKm { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
