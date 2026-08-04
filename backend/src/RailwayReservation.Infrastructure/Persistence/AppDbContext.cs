using Microsoft.EntityFrameworkCore;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Train> Trains => Set<Train>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Journey> Journeys => Set<Journey>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSegment> BookingSegments => Set<BookingSegment>();
    public DbSet<BookingSegmentLeg> BookingSegmentLegs => Set<BookingSegmentLeg>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<FareRule> FareRules => Set<FareRule>();
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
