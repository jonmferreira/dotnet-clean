using System;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ApplicationAuthentication = Parking.Application.Authentication;
using ApplicationSecurity = Parking.Application.Abstractions.Security;
using Parking.Application.Abstractions;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Authentication;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Repositories;

using Parking.Infrastructure.Messaging;
using Parking.Infrastructure.ExternalServices.Cnpja;
using Parking.Application.Abstractions;


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
            .AddOptions<AwsSmsOptions>()
            .Bind(configuration.GetSection(AwsSmsOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Region),
                "AWS SNS region must be provided.")
            .ValidateOnStart();

        services.AddSingleton<IAmazonSimpleNotificationService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsSmsOptions>>().Value;
            var region = RegionEndpoint.GetBySystemName(options.Region);

            if (!string.IsNullOrWhiteSpace(options.AccessKey)
                && !string.IsNullOrWhiteSpace(options.SecretKey))
            {
                var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
                return new AmazonSimpleNotificationServiceClient(credentials, region);
            }

            return new AmazonSimpleNotificationServiceClient(region);
        });

        services.AddSingleton<ISmsSender, AwsSmsSender>();

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

        services
            .AddOptions<CnpjaOptions>()
            .Bind(configuration.GetSection(CnpjaOptions.SectionName))
            .PostConfigure(static options =>
            {
                options.BaseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                    ? CnpjaOptions.DefaultBaseUrl
                    : EnsureEndsWithSlash(options.BaseUrl.Trim());

                options.CompanyEndpoint = string.IsNullOrWhiteSpace(options.CompanyEndpoint)
                    ? CnpjaOptions.DefaultCompanyEndpoint
                    : options.CompanyEndpoint.Trim().TrimStart('/');
            })
            .Validate(
                static options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "CNPJa base URL must be a valid absolute URI.")
            .ValidateOnStart();

        services
            .AddHttpClient<CnpjaOpenApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CnpjaOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });

        services.AddScoped<ICnpjLookupService>(static sp => sp.GetRequiredService<CnpjaOpenApiClient>());

        services.AddScoped<IParkingTicketRepository, ParkingTicketRepository>();
        services.AddScoped<IMonthlyTargetRepository, MonthlyTargetRepository>();
        services.AddScoped<IVehicleInspectionRepository, VehicleInspectionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<Pbkdf2PasswordHasher>();
        services.AddSingleton<ApplicationSecurity.IPasswordHasher>(
            static sp => sp.GetRequiredService<Pbkdf2PasswordHasher>());

        services.AddSingleton<JwtTokenGenerator>();
        services.AddSingleton<ApplicationSecurity.IJwtTokenGenerator>(
            static sp => sp.GetRequiredService<JwtTokenGenerator>());
        services.AddSingleton<ApplicationAuthentication.IJwtTokenGenerator>(
            static sp => sp.GetRequiredService<JwtTokenGenerator>());

        return services;
    }

    private static string EnsureEndsWithSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal)
            ? value
            : string.Concat(value, "/");
    }
}
