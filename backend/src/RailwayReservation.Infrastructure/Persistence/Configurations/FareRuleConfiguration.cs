using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RailwayReservation.Domain.Entities;

namespace RailwayReservation.Infrastructure.Persistence.Configurations;

public class FareRuleConfiguration : IEntityTypeConfiguration<FareRule>
{
    public void Configure(EntityTypeBuilder<FareRule> builder)
    {
        builder.ToTable("fare_rules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(120).IsRequired();
        builder.Property(r => r.CoachType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.RuleType).HasConversion<string>().HasMaxLength(30);
        builder.Property(r => r.ValueType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Value).HasColumnType("decimal(10,4)");
        builder.HasIndex(r => new { r.RuleType, r.IsActive });
    }
}
