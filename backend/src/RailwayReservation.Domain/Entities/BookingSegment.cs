using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Domain.Entities;

public class BookingSegment
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid SeatId { get; set; }
    public Guid JourneyId { get; set; }
    public int OriginOrder { get; set; }
    public int DestinationOrder { get; set; }
    public decimal Fare { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedAt { get; set; }

    public Booking Booking { get; set; } = default!;
    public Seat Seat { get; set; } = default!;
    public Journey Journey { get; set; } = default!;

    /// <summary>
    /// One row per unit leg this segment covers — see BookingSegmentLeg for why this table,
    /// rather than a range type, is what actually guarantees no double-booking on MySQL.
    /// </summary>
    public ICollection<BookingSegmentLeg> Legs { get; set; } = new List<BookingSegmentLeg>();
}
