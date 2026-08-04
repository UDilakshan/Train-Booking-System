using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController, Route("admin/seats"), Authorize]
public class SeatsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<Seat>> FindAll([FromQuery] Guid? coachId)
    {
        var query = db.Seats.OrderBy(s => s.SeatNumber).AsQueryable();
        if (coachId is not null) query = query.Where(s => s.CoachId == coachId);
        return await query.ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<Seat> FindOne(Guid id) =>
        await db.Seats.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundAppException("SEAT_NOT_FOUND", $"Seat {id} was not found.");

    [HttpPost]
    public async Task<Seat> Create(CreateSeatRequest request)
    {
        var seat = new Seat { Id = Guid.NewGuid(), CoachId = request.CoachId, SeatNumber = request.SeatNumber, SeatType = request.SeatType, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Seats.Add(seat);
        await db.SaveChangesAsync();
        return seat;
    }

    [HttpPatch("{id:guid}")]
    public async Task<Seat> Update(Guid id, UpdateSeatRequest request)
    {
        var seat = await db.Seats.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundAppException("SEAT_NOT_FOUND", $"Seat {id} was not found.");
        if (request.SeatNumber is not null) seat.SeatNumber = request.SeatNumber;
        if (request.SeatType is not null) seat.SeatType = request.SeatType;
        seat.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return seat;
    }

    [HttpDelete("{id:guid}")]
    public async Task<Seat> Remove(Guid id)
    {
        var seat = await db.Seats.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundAppException("SEAT_NOT_FOUND", $"Seat {id} was not found.");
        db.Seats.Remove(seat);
        await db.SaveChangesAsync();
        return seat;
    }
}
