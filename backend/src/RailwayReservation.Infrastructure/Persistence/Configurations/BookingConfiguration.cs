using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BookingReference).HasMaxLength(12).IsRequired();
        builder.Property(b => b.PassengerName).HasMaxLength(120).IsRequired();
        builder.Property(b => b.PassengerContact).HasMaxLength(50).IsRequired();
        builder.Property(b => b.TotalFare).HasColumnType("decimal(10,2)");
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(b => b.BookingReference).IsUnique().HasDatabaseName("ux_bookings_booking_reference");
        builder.HasIndex(b => b.JourneyId);

        builder.HasOne(b => b.Journey).WithMany(j => j.Bookings).HasForeignKey(b => b.JourneyId).OnDelete(DeleteBehavior.Restrict);
        // Unidirectional: Station doesn't need collection navigations back to bookings for this app's query patterns.
        builder.HasOne(b => b.OriginStation).WithMany().HasForeignKey(b => b.OriginStationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.DestinationStation).WithMany().HasForeignKey(b => b.DestinationStationId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Segments).WithOne(s => s.Booking).HasForeignKey(s => s.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(b => b.Payments).WithOne(p => p.Booking).HasForeignKey(p => p.BookingId).OnDelete(DeleteBehavior.Cascade);
    }
}
