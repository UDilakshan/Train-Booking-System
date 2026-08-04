using Microsoft.AspNetCore.Mvc;
using RailwayReservation.Application.Availability;
using RailwayReservation.Application.Availability.Dtos;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("availability")]
public class AvailabilityController(GetAvailabilityUseCase getAvailabilityUseCase) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<SeatAvailabilityDto>> Get([FromQuery] Guid journeyId, [FromQuery] Guid originStationId, [FromQuery] Guid destinationStationId) =>
        getAvailabilityUseCase.ExecuteAsync(new AvailabilityRequest(journeyId, originStationId, destinationStationId));
}
