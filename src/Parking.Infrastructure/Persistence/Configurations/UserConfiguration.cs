using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(user => user.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.UpdatedAt);

        builder.HasData(
            new User(
                Guid.Parse("8a2c929f-3a3f-4f67-9d92-56977d042793"),
                "Administrador",
                "admin@parking.local",
                "100000.44HIZV+Wtt2qTDsXM0ThWA==.BI657CNRV2KIplzDWEGCKubiqKEV9K9aU+mWX+yB8Qo=",
                "Admin",
                true,
                new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero))
        );
    }
}
