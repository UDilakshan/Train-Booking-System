using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController]
public class JourneysController(AppDbContext db) : ControllerBase
{
    [HttpGet("journeys")]
    public async Task<IReadOnlyList<Journey>> FindAll([FromQuery] DateOnly? date, [FromQuery] Guid? trainId)
    {
        var query = db.Journeys.Include(j => j.Train).Where(j => j.Status == JourneyStatus.Scheduled).AsQueryable();
        if (date is not null) query = query.Where(j => j.JourneyDate == date);
        if (trainId is not null) query = query.Where(j => j.TrainId == trainId);
        return await query.OrderBy(j => j.JourneyDate).ThenBy(j => j.DepartureTime).ToListAsync();
    }

    [HttpGet("journeys/{id:guid}")]
    public async Task<Journey> FindOne(Guid id) =>
        await db.Journeys.Include(j => j.Train).FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new NotFoundAppException("JOURNEY_NOT_FOUND", $"Journey {id} was not found.");

    [HttpPost("admin/journeys"), Authorize]
    public async Task<Journey> Create(CreateJourneyRequest request)
    {
        var journey = new Journey { Id = Guid.NewGuid(), TrainId = request.TrainId, JourneyDate = request.JourneyDate, DepartureTime = request.DepartureTime, Status = request.Status, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Journeys.Add(journey);
        await db.SaveChangesAsync();
        return journey;
    }

    [HttpPatch("admin/journeys/{id:guid}"), Authorize]
    public async Task<Journey> Update(Guid id, UpdateJourneyRequest request)
    {
        var journey = await db.Journeys.FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new NotFoundAppException("JOURNEY_NOT_FOUND", $"Journey {id} was not found.");
        if (request.JourneyDate is not null) journey.JourneyDate = request.JourneyDate.Value;
        if (request.DepartureTime is not null) journey.DepartureTime = request.DepartureTime;
        if (request.Status is not null) journey.Status = request.Status.Value;
        journey.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return journey;
    }

    [HttpDelete("admin/journeys/{id:guid}"), Authorize]
    public async Task<Journey> Remove(Guid id)
    {
        var journey = await db.Journeys.FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new NotFoundAppException("JOURNEY_NOT_FOUND", $"Journey {id} was not found.");
        db.Journeys.Remove(journey);
        await db.SaveChangesAsync();
        return journey;
    }
}
