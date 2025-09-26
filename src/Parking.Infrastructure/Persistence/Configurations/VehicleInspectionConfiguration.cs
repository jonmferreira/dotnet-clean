using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

public sealed class VehicleInspectionConfiguration : IEntityTypeConfiguration<VehicleInspection>
{
    public void Configure(EntityTypeBuilder<VehicleInspection> builder)
    {
        builder.HasKey(inspection => inspection.Id);

        builder.Property(inspection => inspection.TicketId)
            .IsRequired();

        builder.Property(inspection => inspection.InspectedAt)
            .IsRequired();

        builder.Property(inspection => inspection.ScratchesPhotoUrl)
            .HasMaxLength(1024);

        builder.Property(inspection => inspection.MissingItemsPhotoUrl)
            .HasMaxLength(1024);

        builder.Property(inspection => inspection.LostKeysPhotoUrl)
            .HasMaxLength(1024);

        builder.Property(inspection => inspection.HarshImpactsPhotoUrl)
            .HasMaxLength(1024);

        builder.HasIndex(inspection => inspection.TicketId)
            .IsUnique();

        builder.HasOne(inspection => inspection.Ticket)
            .WithMany()
            .HasForeignKey(inspection => inspection.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
