using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parking.Application.Abstractions.Security;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Repositories;
using Parking.Infrastructure.Authentication;

namespace Parking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddDbContext<ParkingDbContext>(options =>
        {
            options.UseLazyLoadingProxies();
            var provider = configuration["Database:Provider"];
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                    ?? configuration["Database:ConnectionString"];

            if (!string.IsNullOrWhiteSpace(provider)
                && provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "A connection string must be provided when using the SQL Server provider.");
                }

                options.UseSqlServer(connectionString);
                return;
            }

            var databaseName = configuration["Database:Name"] ?? "ParkingDb";
            options.UseInMemoryDatabase(databaseName);
        });

        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();

        services.AddScoped<IMonthlyTargetRepository, MonthlyTargetRepository>();

        services.AddScoped<IVehicleInspectionRepository, VehicleInspectionRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();


        return services;
    }
}
