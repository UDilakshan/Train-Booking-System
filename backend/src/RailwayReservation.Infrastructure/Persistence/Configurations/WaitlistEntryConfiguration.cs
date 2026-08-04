using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("waitlist_entries");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.PassengerName).HasMaxLength(120).IsRequired();
        builder.Property(w => w.PassengerContact).HasMaxLength(50).IsRequired();
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(w => new { w.JourneyId, w.Status });

        builder.HasOne(w => w.Journey).WithMany(j => j.WaitlistEntries).HasForeignKey(w => w.JourneyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(w => w.OriginStation).WithMany().HasForeignKey(w => w.OriginStationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(w => w.DestinationStation).WithMany().HasForeignKey(w => w.DestinationStationId).OnDelete(DeleteBehavior.Restrict);
    }
}
