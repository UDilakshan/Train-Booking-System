using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class CoachConfiguration : IEntityTypeConfiguration<Coach>
{
    public void Configure(EntityTypeBuilder<Coach> builder)
    {
        builder.ToTable("coaches");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CoachNumber).HasMaxLength(10).IsRequired();
        builder.Property(c => c.CoachType).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(c => c.TrainId);
        builder.HasIndex(c => new { c.TrainId, c.CoachNumber }).IsUnique();

        builder.HasMany(c => c.Seats).WithOne(s => s.Coach).HasForeignKey(s => s.CoachId).OnDelete(DeleteBehavior.Cascade);
    }
}
