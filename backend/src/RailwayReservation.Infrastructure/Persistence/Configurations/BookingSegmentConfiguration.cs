using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class BookingSegmentConfiguration : IEntityTypeConfiguration<BookingSegment>
{
    public void Configure(EntityTypeBuilder<BookingSegment> builder)
    {
        builder.ToTable("booking_segments");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Fare).HasColumnType("decimal(10,2)");
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(s => new { s.SeatId, s.JourneyId });
        builder.HasIndex(s => s.BookingId);

        builder.HasOne(s => s.Seat).WithMany(seat => seat.BookingSegments).HasForeignKey(s => s.SeatId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Journey).WithMany(j => j.BookingSegments).HasForeignKey(s => s.JourneyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(s => s.Legs).WithOne(l => l.BookingSegment).HasForeignKey(l => l.BookingSegmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
