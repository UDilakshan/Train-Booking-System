using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("admin/coaches"), Authorize]
public class CoachesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Coach>> FindAll([FromQuery] Guid? trainId)
    {
        var query = db.Coaches.Include(c => c.Seats).OrderBy(c => c.Order).AsQueryable();
        if (trainId is not null) query = query.Where(c => c.TrainId == trainId);
        return await query.ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<Coach> FindOne(Guid id) =>
        await db.Coaches.Include(c => c.Seats).FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundAppException("COACH_NOT_FOUND", $"Coach {id} was not found.");

    [HttpPost]
    public async Task<Coach> Create(CreateCoachRequest request)
    {
        var coach = new Coach { Id = Guid.NewGuid(), TrainId = request.TrainId, CoachNumber = request.CoachNumber, CoachType = request.CoachType, Order = request.Order, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Coaches.Add(coach);

        if (request.SeatCount is { } seatCount)
        {
            for (var i = 1; i <= seatCount; i++)
            {
                db.Seats.Add(new Seat { Id = Guid.NewGuid(), CoachId = coach.Id, SeatNumber = i.ToString("D2"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            }
        }

        await db.SaveChangesAsync();
        return coach;
    }

    [HttpPatch("{id:guid}")]
    public async Task<Coach> Update(Guid id, UpdateCoachRequest request)
    {
        var coach = await db.Coaches.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundAppException("COACH_NOT_FOUND", $"Coach {id} was not found.");
        if (request.CoachNumber is not null) coach.CoachNumber = request.CoachNumber;
        if (request.CoachType is not null) coach.CoachType = request.CoachType.Value;
        if (request.Order is not null) coach.Order = request.Order.Value;
        coach.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return coach;
    }

    [HttpDelete("{id:guid}")]
    public async Task<Coach> Remove(Guid id)
    {
        var coach = await db.Coaches.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundAppException("COACH_NOT_FOUND", $"Coach {id} was not found.");
        db.Coaches.Remove(coach);
        await db.SaveChangesAsync();
        return coach;
    }
}
