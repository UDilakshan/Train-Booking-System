namespace RailwayReservation.Domain.Booking;

/// <summary>
/// The single source of truth for "do these two station-order ranges overlap?" — used by both
/// the Availability engine (read path) and the Bookings use-case (write path, as the fast
/// in-app pre-check ahead of the MySQL UNIQUE-index backstop — see BookingSegmentLeg). A segment
/// occupies stations [OriginOrder, DestinationOrder), so two segments overlap iff one starts
/// before the other ends, in both directions. Equal boundaries (one segment's destination equals
/// the other's origin) are NOT an overlap — that's the "adjacent bookings are allowed" rule
/// (Kandy->NanuOya and NanuOya->Badulla are both valid on the same seat).
/// </summary>
public static class SegmentOverlap
{
    public static bool Overlaps(StationRange a, StationRange b) =>
        a.OriginOrder < b.DestinationOrder && b.OriginOrder < a.DestinationOrder;

    public static bool IsValidRange(StationRange range) =>
        range.OriginOrder >= 0 && range.DestinationOrder > range.OriginOrder;

    /// <summary>The unit legs a range spans — origin, origin+1, ..., destination-1 — matching BookingSegmentLeg.LegOrder.</summary>
    public static IEnumerable<int> Legs(StationRange range)
    {
        for (var leg = range.OriginOrder; leg < range.DestinationOrder; leg++)
        {
            yield return leg;
        }
    }
}
