using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController]
public class TrainsController(AppDbContext db) : ControllerBase
{
    [HttpGet("trains")]
    public async Task<IReadOnlyList<Train>> FindAll() =>
        await db.Trains.Include(t => t.Coaches).OrderBy(t => t.Name).ToListAsync();

    [HttpGet("trains/{id:guid}")]
    public async Task<Train> FindOne(Guid id) =>
        await db.Trains
            .Include(t => t.Coaches.OrderBy(c => c.Order)).ThenInclude(c => c.Seats.OrderBy(s => s.SeatNumber))
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundAppException("TRAIN_NOT_FOUND", $"Train {id} was not found.");

    [HttpPost("admin/trains"), Authorize]
    public async Task<Train> Create(CreateTrainRequest request)
    {
        var train = new Train { Id = Guid.NewGuid(), Code = request.Code.ToUpperInvariant(), Name = request.Name, Description = request.Description, IsExpress = request.IsExpress, IsActive = request.IsActive, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Trains.Add(train);
        await db.SaveChangesAsync();
        return train;
    }

    [HttpPatch("admin/trains/{id:guid}"), Authorize]
    public async Task<Train> Update(Guid id, UpdateTrainRequest request)
    {
        var train = await db.Trains.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundAppException("TRAIN_NOT_FOUND", $"Train {id} was not found.");
        if (request.Code is not null) train.Code = request.Code.ToUpperInvariant();
        if (request.Name is not null) train.Name = request.Name;
        if (request.Description is not null) train.Description = request.Description;
        if (request.IsExpress is not null) train.IsExpress = request.IsExpress.Value;
        if (request.IsActive is not null) train.IsActive = request.IsActive.Value;
        train.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return train;
    }

    [HttpDelete("admin/trains/{id:guid}"), Authorize]
    public async Task<Train> Remove(Guid id)
    {
        var train = await db.Trains.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundAppException("TRAIN_NOT_FOUND", $"Train {id} was not found.");
        db.Trains.Remove(train);
        await db.SaveChangesAsync();
        return train;
    }
}
