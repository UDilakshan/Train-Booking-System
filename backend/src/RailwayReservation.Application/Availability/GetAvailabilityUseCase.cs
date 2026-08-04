using RailwayReservation.Application.Availability.Dtos;
using RailwayReservation.Application.Availability.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Application.Common.Ports;

namespace RailwayReservation.Application.Availability;

public sealed record AvailabilityRequest(Guid JourneyId, Guid OriginStationId, Guid DestinationStationId);

public sealed class GetAvailabilityUseCase(IAvailabilityRepository availabilityRepository, IReferenceDataReader referenceDataReader)
{
    public async Task<IReadOnlyList<SeatAvailabilityDto>> ExecuteAsync(AvailabilityRequest request, CancellationToken ct = default)
    {
        var journey = await referenceDataReader.GetJourneyWithTrainAsync(request.JourneyId, ct)
            ?? throw new NotFoundAppException("JOURNEY_NOT_FOUND", $"Journey {request.JourneyId} was not found.");
        var origin = await referenceDataReader.GetStationAsync(request.OriginStationId, ct)
            ?? throw new NotFoundAppException("STATION_NOT_FOUND", $"Station {request.OriginStationId} was not found.");
        var destination = await referenceDataReader.GetStationAsync(request.DestinationStationId, ct)
            ?? throw new NotFoundAppException("STATION_NOT_FOUND", $"Station {request.DestinationStationId} was not found.");

        if (origin.Order >= destination.Order)
        {
            throw new InvalidSegmentException("Origin station must come before the destination station on the route.");
        }

        var seats = await availabilityRepository.GetSeatsForJourneyTrainAsync(journey.Id, ct);
        var occupiedSeatIds = await availabilityRepository.GetOccupiedSeatIdsAsync(journey.Id, origin.Order, destination.Order, ct);

        return seats
            .Select(seat => new SeatAvailabilityDto(
                seat.SeatId,
                seat.SeatNumber,
                seat.SeatType,
                seat.CoachId,
                seat.CoachNumber,
                seat.CoachType,
                seat.CoachOrder,
                IsAvailable: !occupiedSeatIds.Contains(seat.SeatId)))
            .ToList();
    }
}
