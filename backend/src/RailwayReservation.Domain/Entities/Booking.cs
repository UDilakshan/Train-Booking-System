using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = default!;
    public Guid JourneyId { get; set; }
    public Guid OriginStationId { get; set; }
    public Guid DestinationStationId { get; set; }
    public string PassengerName { get; set; } = default!;
    public string PassengerContact { get; set; } = default!;
    public decimal TotalFare { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Journey Journey { get; set; } = default!;
    public Station OriginStation { get; set; } = default!;
    public Station DestinationStation { get; set; } = default!;
    public ICollection<BookingSegment> Segments { get; set; } = new List<BookingSegment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
