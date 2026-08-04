using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class TrainConfiguration : IEntityTypeConfiguration<Train>
{
    public void Configure(EntityTypeBuilder<Train> builder)
    {
        builder.ToTable("trains");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Code).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(t => t.Code).IsUnique();

        builder.HasMany(t => t.Coaches).WithOne(c => c.Train).HasForeignKey(c => c.TrainId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Journeys).WithOne(j => j.Train).HasForeignKey(j => j.TrainId).OnDelete(DeleteBehavior.Cascade);
    }
}
