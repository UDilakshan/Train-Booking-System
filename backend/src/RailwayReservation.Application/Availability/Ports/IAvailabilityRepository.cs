using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Availability.Ports;

public sealed record SeatRow(Guid SeatId, string SeatNumber, string? SeatType, Guid CoachId, string CoachNumber, CoachType CoachType, int CoachOrder);

public interface IAvailabilityRepository
{
    /// <summary>Every seat belonging to the train that runs <paramref name="journeyId"/>, ordered by coach then seat number.</summary>
    Task<IReadOnlyList<SeatRow>> GetSeatsForJourneyTrainAsync(Guid journeyId, CancellationToken ct = default);

    /// <summary>
    /// Seat ids with at least one occupied leg in [originOrder, destinationOrder) for this
    /// journey — a single indexed range query against BookingSegmentLeg, made possible by the
    /// per-leg occupancy table (see README "Concurrency Strategy"). No overlap function needed
    /// at read time; the write path already decomposed segments into legs.
    /// </summary>
    Task<IReadOnlySet<Guid>> GetOccupiedSeatIdsAsync(Guid journeyId, int originOrder, int destinationOrder, CancellationToken ct = default);
}
