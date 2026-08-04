using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class Journey
{
    public Guid Id { get; set; }
    public Guid TrainId { get; set; }
    public DateOnly JourneyDate { get; set; }
    public string DepartureTime { get; set; } = default!; // "HH:MM"
    public JourneyStatus Status { get; set; } = JourneyStatus.Scheduled;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Train Train { get; set; } = default!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<BookingSegment> BookingSegments { get; set; } = new List<BookingSegment>();
    public ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
}
