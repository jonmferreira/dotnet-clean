using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Repositories;

namespace Parking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ParkingDbContext>(options =>
        {
            var databaseName = configuration["Database:Name"] ?? "ParkingDb";
            options.UseInMemoryDatabase(databaseName);
        });

        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();
        services.AddScoped<IVehicleInspectionRepository, VehicleInspectionRepository>();

        return services;
    }
}
