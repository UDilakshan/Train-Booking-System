using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayReservation.Api.Dtos;
using RailwayReservation.Application.Common.Exceptions;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Api.Controllers;

[ApiController]
public class WaitlistController(AppDbContext db) : ControllerBase
{
    [HttpPost("waitlist")]
    public async Task<ActionResult<WaitlistEntry>> Join(CreateWaitlistEntryRequest request)
    {
        var entry = new WaitlistEntry
        {
            Id = Guid.NewGuid(), JourneyId = request.JourneyId, OriginStationId = request.OriginStationId, DestinationStationId = request.DestinationStationId,
            PassengerName = request.PassengerName, PassengerContact = request.PassengerContact, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        db.WaitlistEntries.Add(entry);
        await db.SaveChangesAsync();
        return StatusCode(201, entry);
    }

    [HttpGet("admin/waitlist"), Authorize]
    public async Task<IReadOnlyList<WaitlistEntry>> FindAll([FromQuery] Guid? journeyId)
    {
        var query = db.WaitlistEntries.OrderBy(w => w.CreatedAt).AsQueryable();
        if (journeyId is not null) query = query.Where(w => w.JourneyId == journeyId);
        return await query.ToListAsync();
    }

    [HttpPatch("admin/waitlist/{id:guid}"), Authorize]
    public async Task<WaitlistEntry> UpdateStatus(Guid id, UpdateWaitlistEntryRequest request)
    {
        var entry = await db.WaitlistEntries.FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new NotFoundAppException("WAITLIST_ENTRY_NOT_FOUND", $"Waitlist entry {id} was not found.");
        entry.Status = request.Status;
        entry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return entry;
    }
}
