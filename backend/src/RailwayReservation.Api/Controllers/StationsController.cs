using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController]
public class StationsController(AppDbContext db) : ControllerBase
{
    [HttpGet("stations")]
    public async Task<IReadOnlyList<Station>> FindAll() =>
        await db.Stations.OrderBy(s => s.Order).ToListAsync();

    [HttpGet("stations/{id:guid}")]
    public async Task<Station> FindOne(Guid id) =>
        await db.Stations.FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundAppException("STATION_NOT_FOUND", $"Station {id} was not found.");

    [HttpPost("admin/stations"), Authorize]
    public async Task<Station> Create(CreateStationRequest request)
    {
        var station = new Station { Id = Guid.NewGuid(), Code = request.Code.ToUpperInvariant(), Name = request.Name, Order = request.Order, DistanceKm = request.DistanceKm, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Stations.Add(station);
        await db.SaveChangesAsync();
        return station;
    }

    [HttpPatch("admin/stations/{id:guid}"), Authorize]
    public async Task<Station> Update(Guid id, UpdateStationRequest request)
    {
        var station = await FindOne(id);
        if (request.Code is not null) station.Code = request.Code.ToUpperInvariant();
        if (request.Name is not null) station.Name = request.Name;
        if (request.Order is not null) station.Order = request.Order.Value;
        if (request.DistanceKm is not null) station.DistanceKm = request.DistanceKm.Value;
        station.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return station;
    }

    [HttpDelete("admin/stations/{id:guid}"), Authorize]
    public async Task<Station> Remove(Guid id)
    {
        var station = await FindOne(id);
        db.Stations.Remove(station);
        await db.SaveChangesAsync();
        return station;
    }
}
