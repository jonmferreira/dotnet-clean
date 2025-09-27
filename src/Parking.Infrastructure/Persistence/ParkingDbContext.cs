using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence;

public sealed class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();


    public DbSet<MonthlyTarget> MonthlyTargets => Set<MonthlyTarget>();

    public DbSet<VehicleInspection> VehicleInspections => Set<VehicleInspection>();

    public DbSet<User> Users => Set<User>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParkingDbContext).Assembly);
    }
}
