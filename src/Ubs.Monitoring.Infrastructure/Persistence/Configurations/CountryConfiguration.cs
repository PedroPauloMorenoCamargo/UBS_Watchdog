using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ubs.Monitoring.Domain.Entities;
using Ubs.Monitoring.Domain.Enums;

namespace Ubs.Monitoring.Infrastructure.Persistence.Configurations;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");

        builder.HasKey(c => c.Code);

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(2)")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(c => c.RiskLevel)
            .HasColumnName("risk_level")
            .HasDefaultValue(RiskLevel.Low)
            .IsRequired();

        // Index for searching by name (optional, for future use)
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_countries_name");

        // Index for filtering by risk level
        builder.HasIndex(c => c.RiskLevel)
            .HasDatabaseName("IX_countries_risk_level");
    }
}
