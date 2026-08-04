using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

/// <summary>
/// This is the concurrency-critical table — see README "Concurrency Strategy" and
/// BookingSegmentLeg's doc comment. The UNIQUE index below is the hard database invariant that
/// makes double-booking a seat/leg impossible, MySQL's stand-in for Postgres's
/// EXCLUDE USING gist constraint.
/// </summary>
public class BookingSegmentLegConfiguration : IEntityTypeConfiguration<BookingSegmentLeg>
{
    public void Configure(EntityTypeBuilder<BookingSegmentLeg> builder)
    {
        builder.ToTable("booking_segment_legs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.HasIndex(l => new { l.SeatId, l.JourneyId, l.LegOrder })
            .IsUnique()
            .HasDatabaseName("ux_booking_segment_legs_seat_journey_leg");

        builder.HasOne(l => l.BookingSegment)
            .WithMany(s => s.Legs)
            .HasForeignKey(l => l.BookingSegmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
