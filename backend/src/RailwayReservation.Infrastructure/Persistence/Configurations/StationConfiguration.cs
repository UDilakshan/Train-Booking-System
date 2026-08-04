using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("stations");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(120).IsRequired();
        builder.Property(s => s.DistanceKm).HasColumnType("decimal(8,2)");
        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.Order).IsUnique();
    }
}
