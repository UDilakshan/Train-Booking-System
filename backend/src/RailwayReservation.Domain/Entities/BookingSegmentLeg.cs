namespace RailwayReservation.Domain.Entities;

/// <summary>
/// The MySQL-native replacement for PostgreSQL's <c>EXCLUDE USING gist</c> range constraint —
/// see README "Concurrency Strategy". A CONFIRMED <see cref="BookingSegment"/> spanning station
/// orders [originOrder, destinationOrder) inserts one row here per unit leg it covers
/// (LegOrder = originOrder .. destinationOrder-1). A plain UNIQUE(SeatId, JourneyId, LegOrder)
/// index then makes "two segments overlap" and "duplicate key on insert" the same event, which
/// InnoDB enforces atomically — no range types required.
///
/// Rows exist only for *currently occupied* legs: cancelling a booking deletes its leg rows
/// (MySQL has no partial/filtered unique index to scope the constraint to CONFIRMED rows the
/// way Postgres's WHERE clause did), while the parent BookingSegment is soft-cancelled for
/// history/audit.
/// </summary>
public class BookingSegmentLeg
{
    public long Id { get; set; }
    public Guid BookingSegmentId { get; set; }
    public Guid SeatId { get; set; }
    public Guid JourneyId { get; set; }
    public int LegOrder { get; set; }

    public BookingSegment BookingSegment { get; set; } = default!;
}
