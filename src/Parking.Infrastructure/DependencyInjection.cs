using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ApplicationAuthentication = Parking.Application.Authentication;
using ApplicationSecurity = Parking.Application.Abstractions.Security;
using Parking.Application.Abstractions;
using Parking.Application.Options;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Authentication;
using Parking.Infrastructure.Email;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Repositories;

namespace Parking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SecretKey) && options.SecretKey.Length >= 16,
                "JWT secret key must be provided and contain at least 16 characters.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "JWT issuer must be provided.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "JWT audience must be provided.")
            .Validate(
                options => options.AccessTokenExpirationMinutes > 0,
                "JWT expiration must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<PasswordResetOptions>()
            .Bind(configuration.GetSection("PasswordReset"))
            .Validate(
                options => options.TokenExpirationMinutes > 0,
                "Password reset token expiration must be greater than zero.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ResetUrl),
                "Password reset URL must be configured.")
            .ValidateOnStart();

        services
            .AddOptions<AwsSesOptions>()
            .Bind(configuration.GetSection("Email:AwsSes"))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Region),
                "AWS region must be configured for SES.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.FromAddress),
                "SES sender address must be configured.")
            .ValidateOnStart();

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
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

        services.AddSingleton<Pbkdf2PasswordHasher>();
        services.AddSingleton<ApplicationSecurity.IPasswordHasher>(
            static sp => sp.GetRequiredService<Pbkdf2PasswordHasher>());

        services.AddSingleton<JwtTokenGenerator>();
        services.AddSingleton<ApplicationSecurity.IJwtTokenGenerator>(
            static sp => sp.GetRequiredService<JwtTokenGenerator>());
        services.AddSingleton<ApplicationAuthentication.IJwtTokenGenerator>(
            static sp => sp.GetRequiredService<JwtTokenGenerator>());

        services.AddSingleton<IEmailSender, AwsSesEmailSender>();

        return services;
    }
}
