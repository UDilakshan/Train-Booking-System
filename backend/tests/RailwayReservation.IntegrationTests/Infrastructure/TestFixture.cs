using Microsoft.EntityFrameworkCore;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.IntegrationTests.Infrastructure;

/// <summary>
/// Self-contained fixture data — uses station orders (1000+) and a dedicated train code well
/// outside the seed script's range, so these tests don't collide with `dotnet run -- seed`
/// having been run against the same database, and don't depend on it either.
/// A(0) -> B(1) -> C(2) -> D(3), analogous to Colombo->Kandy->NanuOya->Badulla.
/// </summary>
public sealed class TestFixtureData
{
    public required Guid StationA { get; init; }
    public required Guid StationB { get; init; }
    public required Guid StationC { get; init; }
    public required Guid StationD { get; init; }
    public required Guid TrainId { get; init; }
    public required Guid CoachId { get; init; }
    public required List<Guid> SeatIds { get; init; }
    public required Guid JourneyId { get; init; }
}

public static class TestFixture
{
    public static async Task<TestFixtureData> SeedAsync(AppDbContext db)
    {
        var stationDefs = new (string Code, string Name, int Order, decimal DistanceKm)[]
        {
            ("E2E-A", "E2E Station A", 1000, 0m),
            ("E2E-B", "E2E Station B", 1001, 50m),
            ("E2E-C", "E2E Station C", 1002, 120m),
            ("E2E-D", "E2E Station D", 1003, 200m),
        };

        var stationIds = new List<Guid>();
        foreach (var s in stationDefs)
        {
            var existing = await db.Stations.FirstOrDefaultAsync(x => x.Code == s.Code);
            if (existing is null)
            {
                existing = new Station { Id = Guid.NewGuid(), Code = s.Code, Name = s.Name, Order = s.Order, DistanceKm = s.DistanceKm, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                db.Stations.Add(existing);
                await db.SaveChangesAsync();
            }
            stationIds.Add(existing.Id);
        }

        var train = await db.Trains.FirstOrDefaultAsync(t => t.Code == "E2E-TRAIN");
        if (train is null)
        {
            train = new Train { Id = Guid.NewGuid(), Code = "E2E-TRAIN", Name = "E2E Test Express", IsExpress = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Trains.Add(train);
            await db.SaveChangesAsync();
        }

        var coach = await db.Coaches.FirstOrDefaultAsync(c => c.TrainId == train.Id && c.CoachNumber == "T1");
        if (coach is null)
        {
            coach = new Coach { Id = Guid.NewGuid(), TrainId = train.Id, CoachNumber = "T1", CoachType = CoachType.SecondClass, Order = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Coaches.Add(coach);
            await db.SaveChangesAsync();
        }

        var seatIds = new List<Guid>();
        foreach (var seatNumber in new[] { "01", "02", "03" })
        {
            var seat = await db.Seats.FirstOrDefaultAsync(s => s.CoachId == coach.Id && s.SeatNumber == seatNumber);
            if (seat is null)
            {
                seat = new Seat { Id = Guid.NewGuid(), CoachId = coach.Id, SeatNumber = seatNumber, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                db.Seats.Add(seat);
                await db.SaveChangesAsync();
            }
            seatIds.Add(seat.Id);
        }

        // Far-future fixed date so it never collides with the seed script's rolling 14-day window.
        var journeyDate = new DateOnly(2099, 6, 15);
        var journey = await db.Journeys.FirstOrDefaultAsync(j => j.TrainId == train.Id && j.JourneyDate == journeyDate);
        if (journey is null)
        {
            journey = new Journey { Id = Guid.NewGuid(), TrainId = train.Id, JourneyDate = journeyDate, DepartureTime = "10:00", Status = JourneyStatus.Scheduled, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Journeys.Add(journey);
            await db.SaveChangesAsync();
        }

        var hasBaseRule = await db.FareRules.AnyAsync(r => r.RuleType == FareRuleType.Base && r.CoachType == null && r.IsActive);
        if (!hasBaseRule)
        {
            db.FareRules.Add(new FareRule { Id = Guid.NewGuid(), Name = "E2E fallback base fare", RuleType = FareRuleType.Base, ValueType = FareValueType.PerKm, Value = 5m, Priority = 0, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        return new TestFixtureData
        {
            StationA = stationIds[0],
            StationB = stationIds[1],
            StationC = stationIds[2],
            StationD = stationIds[3],
            TrainId = train.Id,
            CoachId = coach.Id,
            SeatIds = seatIds,
            JourneyId = journey.Id,
        };
    }

    /// <summary>Clears bookings created by a test run, keeping reference data intact for reuse.</summary>
    public static async Task ResetBookingsAsync(AppDbContext db, TestFixtureData fixture)
    {
        var segmentIds = await db.BookingSegments.Where(s => s.JourneyId == fixture.JourneyId).Select(s => s.Id).ToListAsync();
        await db.BookingSegmentLegs.Where(l => segmentIds.Contains(l.BookingSegmentId)).ExecuteDeleteAsync();
        await db.BookingSegments.Where(s => s.JourneyId == fixture.JourneyId).ExecuteDeleteAsync();
        await db.Bookings.Where(b => b.JourneyId == fixture.JourneyId).ExecuteDeleteAsync();
    }
}
