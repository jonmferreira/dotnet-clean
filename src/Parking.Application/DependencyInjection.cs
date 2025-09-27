using Microsoft.Extensions.DependencyInjection;
using Parking.Application.Abstractions;
using Parking.Application.Services;
using Parking.Domain.Services;

namespace Parking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IParkingTicketService, ParkingTicketService>();

        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        services.AddScoped<IVehicleInspectionService, VehicleInspectionService>();


        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IPasswordResetService, PasswordResetService>();

        services.AddScoped<ITicketDetailsService, TicketDetailsService>();


        services.AddSingleton<IParkingFeeCalculator, ParkingFeeCalculator>();
        return services;
    }
}
