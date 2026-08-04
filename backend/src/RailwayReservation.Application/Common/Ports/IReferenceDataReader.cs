using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Common.Ports;

/// <summary>
/// Narrow read-only lookups shared by the Bookings/Availability/Fare use-cases. Deliberately not
/// a full repository — reference-data CRUD (stations/trains/coaches/seats/journeys/fare-rules)
/// lives directly in the Api layer against the DbContext (see README "Clean Architecture
/// scoping"), but the *complex* use-cases still need read access to that data without the
/// Application layer referencing Infrastructure/EF Core directly, hence this seam.
/// </summary>
public interface IReferenceDataReader
{
    Task<Station?> GetStationAsync(Guid id, CancellationToken ct = default);

    Task<Journey?> GetJourneyWithTrainAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Seat>> GetSeatsWithCoachAsync(IReadOnlyCollection<Guid> seatIds, CancellationToken ct = default);
}
