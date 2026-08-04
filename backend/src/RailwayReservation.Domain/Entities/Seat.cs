namespace RailwayReservation.Domain.Entities;

public class Seat
{
    public Guid Id { get; set; }
    public Guid CoachId { get; set; }
    public string SeatNumber { get; set; } = default!;
    public string? SeatType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Coach Coach { get; set; } = default!;
    public ICollection<BookingSegment> BookingSegments { get; set; } = new List<BookingSegment>();
}
