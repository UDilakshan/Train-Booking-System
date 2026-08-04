using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RailwayReservation.Domain.Entities;
using RailwayReservation.Domain.Enums;
using RailwayReservation.Infrastructure.Auth;
using RailwayReservation.Infrastructure.Persistence;

namespace RailwayReservation.Infrastructure.Seed;

/// <summary>
/// All of this is *seed data*, not application logic — station lists, coach layouts, and fare
/// rules are ordinary rows an admin could otherwise create through the /admin CRUD endpoints.
/// Nothing here is read by app code. Mirrors the original Node build's prisma/seed.ts.
/// </summary>
public static class DbSeeder
{
    private sealed record StationSeed(string Code, string Name, int Order, decimal DistanceKm);

    private sealed record CoachSeed(string CoachNumber, CoachType CoachType, int Order, int SeatCount);

    private sealed record TrainSeed(string Code, string Name, string Description, bool IsExpress, string DepartureTime);

    // Approximate real-world distances (km from Colombo Fort) along the iconic upcountry line.
    private static readonly StationSeed[] Stations =
    [
        new("CMB", "Colombo Fort", 0, 0m),
        new("MRD", "Maradana", 1, 1.6m),
        new("RGM", "Ragama", 2, 15.2m),
        new("GPH", "Gampaha", 3, 25.6m),
        new("VYG", "Veyangoda", 4, 36.6m),
        new("PLG", "Polgahawela", 5, 71.2m),
        new("RMB", "Rambukkana", 6, 87.0m),
        new("KDG", "Kadugannawa", 7, 101.4m),
        new("PRD", "Peradeniya", 8, 111.8m),
        new("KDY", "Kandy", 9, 121.0m),
        new("GMP", "Gampola", 10, 138.0m),
        new("NWL", "Nawalapitiya", 11, 155.0m),
        new("HTN", "Hatton", 12, 187.0m),
        new("TLW", "Talawakele", 13, 204.0m),
        new("NOY", "Nanu Oya", 14, 222.0m),
        new("HPT", "Haputale", 15, 246.0m),
        new("BDW", "Bandarawela", 16, 260.0m),
        new("ELA", "Ella", 17, 273.0m),
        new("BDL", "Badulla", 18, 293.0m),
    ];

    private static readonly CoachSeed[] CoachLayout =
    [
        new("A1", CoachType.FirstClass, 1, 20),
        new("B1", CoachType.SecondClass, 2, 40),
        new("B2", CoachType.SecondClass, 3, 40),
        new("C1", CoachType.ThirdClass, 4, 60),
        new("C2", CoachType.ThirdClass, 5, 60),
        new("C3", CoachType.ThirdClass, 6, 60),
    ];

    private static readonly TrainSeed[] Trains =
    [
        new("PODI-MENIKE", "Podi Menike", "Intercity Express, Colombo Fort — Badulla", true, "05:55"),
        new("BADULLA-MAIL", "Badulla Night Mail", "Overnight mail train, Colombo Fort — Badulla", false, "20:00"),
    ];

    public static async Task SeedAsync(AppDbContext db, IOptions<JwtOptions> jwtOptions)
    {
        await SeedStationsAsync(db);
        await SeedAdminUserAsync(db);
        await SeedTrainsCoachesAndJourneysAsync(db);
        await SeedFareRulesAsync(db);
    }

    private static async Task SeedStationsAsync(AppDbContext db)
    {
        foreach (var s in Stations)
        {
            var existing = await db.Stations.FirstOrDefaultAsync(x => x.Code == s.Code);
            if (existing is null)
            {
                db.Stations.Add(new Station { Id = Guid.NewGuid(), Code = s.Code, Name = s.Name, Order = s.Order, DistanceKm = s.DistanceKm, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
            }
            else
            {
                existing.Name = s.Name;
                existing.Order = s.Order;
                existing.DistanceKm = s.DistanceKm;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"Seeded {Stations.Length} stations.");
    }

    private static async Task SeedAdminUserAsync(AppDbContext db)
    {
        var email = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL") ?? "admin@railway.lk";
        var password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? "ChangeMe123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existing is null)
        {
            db.Users.Add(new User { Id = Guid.NewGuid(), Email = email, Name = "System Administrator", Role = UserRole.Admin, PasswordHash = passwordHash, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        }
        else
        {
            existing.PasswordHash = passwordHash;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"Seeded admin user {email}.");
    }

    private static async Task SeedTrainsCoachesAndJourneysAsync(AppDbContext db)
    {
        foreach (var trainSeed in Trains)
        {
            var train = await db.Trains.FirstOrDefaultAsync(t => t.Code == trainSeed.Code);
            if (train is null)
            {
                train = new Train { Id = Guid.NewGuid(), Code = trainSeed.Code, CreatedAt = DateTime.UtcNow };
                db.Trains.Add(train);
            }
            train.Name = trainSeed.Name;
            train.Description = trainSeed.Description;
            train.IsExpress = trainSeed.IsExpress;
            train.IsActive = true;
            train.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            foreach (var coachSeed in CoachLayout)
            {
                var coach = await db.Coaches.FirstOrDefaultAsync(c => c.TrainId == train.Id && c.CoachNumber == coachSeed.CoachNumber);
                if (coach is null)
                {
                    coach = new Coach { Id = Guid.NewGuid(), TrainId = train.Id, CoachNumber = coachSeed.CoachNumber, CreatedAt = DateTime.UtcNow };
                    db.Coaches.Add(coach);
                }
                coach.CoachType = coachSeed.CoachType;
                coach.Order = coachSeed.Order;
                coach.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();

                for (var i = 1; i <= coachSeed.SeatCount; i++)
                {
                    var seatNumber = i.ToString("D2");
                    var seatExists = await db.Seats.AnyAsync(s => s.CoachId == coach.Id && s.SeatNumber == seatNumber);
                    if (!seatExists)
                    {
                        db.Seats.Add(new Seat { Id = Guid.NewGuid(), CoachId = coach.Id, SeatNumber = seatNumber, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
                    }
                }
                await db.SaveChangesAsync();
            }

            // Seed the next 14 days of journeys so the demo always has bookable dates regardless of when this runs.
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            for (var dayOffset = 0; dayOffset < 14; dayOffset++)
            {
                var journeyDate = today.AddDays(dayOffset);
                var journey = await db.Journeys.FirstOrDefaultAsync(j => j.TrainId == train.Id && j.JourneyDate == journeyDate);
                if (journey is null)
                {
                    db.Journeys.Add(new Journey { Id = Guid.NewGuid(), TrainId = train.Id, JourneyDate = journeyDate, DepartureTime = trainSeed.DepartureTime, Status = JourneyStatus.Scheduled, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
                }
                else
                {
                    journey.DepartureTime = trainSeed.DepartureTime;
                    journey.Status = JourneyStatus.Scheduled;
                    journey.UpdatedAt = DateTime.UtcNow;
                }
            }
            await db.SaveChangesAsync();

            Console.WriteLine($"Seeded train {train.Name} with {CoachLayout.Length} coaches and 14 journeys.");
        }
    }

    private static async Task SeedFareRulesAsync(AppDbContext db)
    {
        // Idempotent: wipe and recreate the demo rule set so re-seeding is safe.
        db.FareRules.RemoveRange(db.FareRules);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        db.FareRules.AddRange(
            new FareRule { Id = Guid.NewGuid(), Name = "Base fare (fallback, all classes)", CoachType = null, RuleType = FareRuleType.Base, ValueType = FareValueType.PerKm, Value = 7.5m, Priority = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new FareRule { Id = Guid.NewGuid(), Name = "Base fare — First Class", CoachType = CoachType.FirstClass, RuleType = FareRuleType.Base, ValueType = FareValueType.PerKm, Value = 15.0m, Priority = 10, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new FareRule { Id = Guid.NewGuid(), Name = "Base fare — Second Class", CoachType = CoachType.SecondClass, RuleType = FareRuleType.Base, ValueType = FareValueType.PerKm, Value = 10.0m, Priority = 10, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new FareRule { Id = Guid.NewGuid(), Name = "Base fare — Third Class", CoachType = CoachType.ThirdClass, RuleType = FareRuleType.Base, ValueType = FareValueType.PerKm, Value = 6.0m, Priority = 10, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new FareRule { Id = Guid.NewGuid(), Name = "Peak hour surcharge", CoachType = null, RuleType = FareRuleType.Peak, ValueType = FareValueType.Percent, Value = 15m, Priority = 0, IsActive = true, CreatedAt = now, UpdatedAt = now },
            new FareRule { Id = Guid.NewGuid(), Name = "Express service surcharge", CoachType = null, RuleType = FareRuleType.Express, ValueType = FareValueType.Percent, Value = 20m, Priority = 0, IsActive = true, CreatedAt = now, UpdatedAt = now });

        await db.SaveChangesAsync();
        Console.WriteLine("Seeded fare rules.");
    }
}
