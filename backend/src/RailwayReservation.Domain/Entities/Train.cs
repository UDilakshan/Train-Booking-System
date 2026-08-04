namespace RailwayReservation.Domain.Entities;

public class Train
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsExpress { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Coach> Coaches { get; set; } = new List<Coach>();
    public ICollection<Journey> Journeys { get; set; } = new List<Journey>();
}
