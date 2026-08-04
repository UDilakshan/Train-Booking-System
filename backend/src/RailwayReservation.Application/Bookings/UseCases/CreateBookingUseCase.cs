using RailwayReservation.Application.Bookings.Dtos;
using RailwayReservation.Application.Bookings.Ports;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Application.Common.Ports;
using RailwayReservation.Application.Fare;
using RailwayReservation.Domain.Booking;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Bookings.UseCases;

public sealed class CreateBookingUseCase(
    IBookingRepository bookingRepository,
    IReferenceDataReader referenceDataReader,
    FareCalculationService fareCalculationService)
{
    private const int MaxReferenceCollisionRetries = 3;

    public async Task<Booking> ExecuteAsync(CreateBookingRequest request, CancellationToken ct = default)
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
        if (!SegmentOverlap.IsValidRange(new StationRange(origin.Order, destination.Order)))
        {
            throw new InvalidSegmentException("Invalid station range.");
        }

        var uniqueSeatIds = request.SeatIds.Distinct().ToList();
        var seats = await referenceDataReader.GetSeatsWithCoachAsync(uniqueSeatIds, ct);
        if (seats.Count != uniqueSeatIds.Count)
        {
            throw new InvalidSegmentException("One or more selected seats do not exist.");
        }
        if (seats.Any(seat => seat.Coach.TrainId != journey.TrainId))
        {
            throw new InvalidSegmentException("One or more selected seats do not belong to this journey's train.");
        }

        var segments = new List<CreateBookingSegmentCommand>();
        var totalFare = 0m;
        foreach (var seat in seats)
        {
            var fare = await fareCalculationService.CalculateAsync(
                new FareQuoteInput(origin.DistanceKm, destination.DistanceKm, seat.Coach.CoachType, journey.DepartureTime, journey.Train.IsExpress, journey.JourneyDate.ToDateTime(TimeOnly.MinValue)),
                ct);
            segments.Add(new CreateBookingSegmentCommand(seat.Id, fare));
            totalFare += fare;
        }

        var command = new CreateBookingCommand(
            journey.Id,
            origin.Id,
            destination.Id,
            origin.Order,
            destination.Order,
            request.PassengerName,
            request.PassengerContact,
            BookingReference: string.Empty, // filled in per-attempt below
            Math.Round(totalFare, 2, MidpointRounding.AwayFromZero),
            segments);

        for (var attempt = 0; attempt < MaxReferenceCollisionRetries; attempt++)
        {
            try
            {
                return await bookingRepository.CreateAsync(command with { BookingReference = BookingReferenceGenerator.Generate() }, ct);
            }
            catch (ConflictAppException ex) when (ex.Code == "DUPLICATE_BOOKING_REFERENCE")
            {
                if (attempt == MaxReferenceCollisionRetries - 1) throw;
            }
        }

        throw new InvalidOperationException("Unreachable.");
    }
}
