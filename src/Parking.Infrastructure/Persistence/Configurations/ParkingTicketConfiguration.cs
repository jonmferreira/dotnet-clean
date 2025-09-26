using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

public sealed class ParkingTicketConfiguration : IEntityTypeConfiguration<ParkingTicket>
{
    public void Configure(EntityTypeBuilder<ParkingTicket> builder)
    {
        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Plate)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(ticket => ticket.EntryAt)
            .IsRequired();

        builder.Property(ticket => ticket.ExitAt);

        builder.Property(ticket => ticket.TotalAmount);

        builder.HasIndex(ticket => new { ticket.Plate, ticket.ExitAt });
    }
}
