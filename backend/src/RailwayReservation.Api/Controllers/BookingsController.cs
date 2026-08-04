using Microsoft.AspNetCore.Mvc;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Bookings.Dtos;
using RailwayReservation.Application.Bookings.UseCases;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("bookings")]
public class BookingsController(
    CreateBookingUseCase createBookingUseCase,
    GetBookingUseCase getBookingUseCase,
    CancelBookingUseCase cancelBookingUseCase,
    UpdateBookingUseCase updateBookingUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequestDto request)
    {
        var booking = await createBookingUseCase.ExecuteAsync(new CreateBookingRequest(
            request.JourneyId, request.OriginStationId, request.DestinationStationId, request.PassengerName, request.PassengerContact, request.SeatIds));
        return StatusCode(201, BookingMapper.ToDto(booking));
    }

    [HttpGet("{reference}")]
    public async Task<BookingDto> FindOne(string reference) => BookingMapper.ToDto(await getBookingUseCase.ExecuteAsync(reference));

    [HttpPatch("{reference}")]
    public async Task<BookingDto> Update(string reference, UpdateBookingRequestDto request) =>
        BookingMapper.ToDto(await updateBookingUseCase.ExecuteAsync(reference, new UpdateBookingRequest(request.PassengerName, request.PassengerContact)));

    [HttpDelete("{reference}")]
    public async Task<BookingDto> Cancel(string reference) => BookingMapper.ToDto(await cancelBookingUseCase.ExecuteAsync(reference));
}
