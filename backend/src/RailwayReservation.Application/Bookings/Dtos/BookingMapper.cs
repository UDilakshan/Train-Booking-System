using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Application.Bookings.Dtos;

public static class BookingMapper
{
    public static BookingDto ToDto(Booking booking) => new(
        booking.Id,
        booking.BookingReference,
        booking.JourneyId,
        booking.Journey.JourneyDate,
        booking.Journey.DepartureTime,
        booking.Journey.Train.Name,
        booking.OriginStationId,
        booking.OriginStation.Name,
        booking.DestinationStationId,
        booking.DestinationStation.Name,
        booking.PassengerName,
        booking.PassengerContact,
        booking.TotalFare,
        booking.Status,
        booking.CreatedAt,
        booking.Segments.Select(s => new BookingSegmentDto(
            s.Id,
            s.SeatId,
            s.Seat.SeatNumber,
            s.Seat.CoachId,
            s.Seat.Coach.CoachNumber,
            s.Seat.Coach.CoachType,
            s.OriginOrder,
            s.DestinationOrder,
            s.Fare,
            s.Status)).ToList());
}
