using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class JourneyConfiguration : IEntityTypeConfiguration<Journey>
{
    public void Configure(EntityTypeBuilder<Journey> builder)
    {
        builder.ToTable("journeys");
        builder.HasKey(j => j.Id);
        builder.Property(j => j.DepartureTime).HasMaxLength(5).IsRequired();
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(j => j.JourneyDate);
        builder.HasIndex(j => new { j.TrainId, j.JourneyDate }).IsUnique();
    }
}
