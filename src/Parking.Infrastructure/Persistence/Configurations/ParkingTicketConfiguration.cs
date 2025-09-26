using System;
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

        builder.HasData(
            new
            {
                Id = Guid.Parse("2f8de7eb-1f5f-4cf6-b1ff-8a669cb6f20b"),
                Plate = "ABC1D23",
                EntryAt = new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero),
                ExitAt = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
                TotalAmount = 20m
            },
            new
            {
                Id = Guid.Parse("b3057559-6fa2-40b1-950c-2fa98cfaa0e5"),
                Plate = "XYZ9B76",
                EntryAt = new DateTimeOffset(2024, 1, 5, 9, 0, 0, TimeSpan.Zero),
                ExitAt = new DateTimeOffset(2024, 1, 5, 12, 30, 0, TimeSpan.Zero),
                TotalAmount = 45m
            },
            new
            {
                Id = Guid.Parse("5f97197f-2f2d-4b6e-af5f-7e77a785bb8f"),
                Plate = "LMN1A11",
                EntryAt = new DateTimeOffset(2024, 1, 10, 7, 30, 0, TimeSpan.Zero),
                ExitAt = new DateTimeOffset(2024, 1, 10, 9, 0, 0, TimeSpan.Zero),
                TotalAmount = 18m
            },
            new
            {
                Id = Guid.Parse("8c910afe-f0f2-4bb3-9f9d-569ed9493a6e"),
                Plate = "OPQ2C22",
                EntryAt = new DateTimeOffset(2024, 2, 2, 14, 0, 0, TimeSpan.Zero),
                ExitAt = new DateTimeOffset(2024, 2, 2, 16, 0, 0, TimeSpan.Zero),
                TotalAmount = 55m
            },
            new
            {
                Id = Guid.Parse("5a74b314-fb50-46a8-b675-8cc0b4fd9033"),
                Plate = "RST3D33",
                EntryAt = new DateTimeOffset(2024, 2, 15, 18, 0, 0, TimeSpan.Zero),
                ExitAt = new DateTimeOffset(2024, 2, 15, 19, 15, 0, TimeSpan.Zero),
                TotalAmount = 28m
            },
            new
            {
                Id = Guid.Parse("c6802e2a-6d36-4396-8fa6-d7313f2c9f8e"),
                Plate = "UVW4E44",
                EntryAt = new DateTimeOffset(2024, 3, 1, 8, 15, 0, TimeSpan.Zero),
                ExitAt = (DateTimeOffset?)null,
                TotalAmount = (decimal?)null
            }
        );
    }
}
