using Microsoft.AspNetCore.Mvc;
using RailwayReservation.Application.Fare;
using RailwayReservation.Domain.Enums;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("fare")]
public class FareController(GetFareQuoteUseCase getFareQuoteUseCase) : ControllerBase
{
    [HttpGet("quote")]
    public Task<FareQuoteResult> Quote([FromQuery] Guid journeyId, [FromQuery] Guid originStationId, [FromQuery] Guid destinationStationId, [FromQuery] CoachType coachType) =>
        getFareQuoteUseCase.ExecuteAsync(new FareQuoteRequest(journeyId, originStationId, destinationStationId, coachType));
}
