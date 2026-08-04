using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SeatNumber).HasMaxLength(10).IsRequired();
        builder.Property(s => s.SeatType).HasMaxLength(20);
        builder.HasIndex(s => s.CoachId);
        builder.HasIndex(s => new { s.CoachId, s.SeatNumber }).IsUnique();
    }
}
