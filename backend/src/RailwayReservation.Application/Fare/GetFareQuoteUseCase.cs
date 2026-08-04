using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Application.Common.Ports;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Application.Fare;

public sealed record FareQuoteRequest(Guid JourneyId, Guid OriginStationId, Guid DestinationStationId, CoachType CoachType);

public sealed record FareQuoteResult(decimal Fare, decimal DistanceKm, string Currency = "LKR");

public sealed class GetFareQuoteUseCase(IReferenceDataReader referenceDataReader, FareCalculationService fareCalculationService)
{
    public async Task<FareQuoteResult> ExecuteAsync(FareQuoteRequest request, CancellationToken ct = default)
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

        var fare = await fareCalculationService.CalculateAsync(
            new FareQuoteInput(origin.DistanceKm, destination.DistanceKm, request.CoachType, journey.DepartureTime, journey.Train.IsExpress, journey.JourneyDate.ToDateTime(TimeOnly.MinValue)),
            ct);

        return new FareQuoteResult(fare, destination.DistanceKm - origin.DistanceKm);
    }
}
