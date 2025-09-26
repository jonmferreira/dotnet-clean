using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

public sealed class MonthlyTargetConfiguration : IEntityTypeConfiguration<MonthlyTarget>
{
    public void Configure(EntityTypeBuilder<MonthlyTarget> builder)
    {
        builder.HasKey(target => target.Id);

        builder.Property(target => target.Year)
            .IsRequired();

        builder.Property(target => target.Month)
            .IsRequired();

        builder.Property(target => target.TargetEntries)
            .IsRequired();

        builder.HasIndex(target => new { target.Year, target.Month })
            .IsUnique();
    }
}
