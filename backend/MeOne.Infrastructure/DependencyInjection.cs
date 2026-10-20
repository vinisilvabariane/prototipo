using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;
using MeOne.Infrastructure.Data.Context;
using MeOne.Infrastructure.Email;
using MeOne.Infrastructure.HealthChecks;
using MeOne.Infrastructure.Identity;
using MeOne.Infrastructure.Oee;
using MeOne.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeOne.Infrastructure;

public static class DependencyInjection
{

    public const string LiveTag = "live";

    public const string ReadyTag = "ready";

    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services) =>
        services.AddByConvention(typeof(DependencyInjection).Assembly, "Repository");

    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services) =>
        services.AddByConvention(typeof(ICurrentActor).Assembly, "UseCase");

    private static IServiceCollection AddByConvention(
        this IServiceCollection services,
        System.Reflection.Assembly assembly,
        string suffix)
    {
        var implementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .Where(type => type.Name.EndsWith(suffix, StringComparison.Ordinal));

        foreach (var implementation in implementations)
        {
            var contract = implementation.GetInterfaces()
                .FirstOrDefault(candidate => candidate.Name == "I" + implementation.Name);

            if (contract is not null)
                services.AddScoped(contract, implementation);
        }

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMeOneDatabase(configuration);
        services.AddIdentity(configuration);
        services.AddWorkflowStorage(configuration);
        services.AddScoped<IOeeLossSource, HelpdeskOeeLossSource>();
        services.AddMeOneHealthChecks(configuration);

        return services;
    }

    private static void AddMeOneDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = MeOneConnectionString(configuration);

        services.AddDbContext<MeOneDbContext>(options =>
            options.UseMySql(connectionString, MeOneDbContext.ServerVersion));
    }

    private const string JwtSectionName = "Jwt";

    private static void AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();

        var refreshHours = configuration.GetValue(
            $"{JwtSectionName}:RefreshTokenExpirationInHours",
            SessionPolicy.Default.RefreshTokenLifetime.TotalHours);

        var session = new SessionPolicy(TimeSpan.FromHours(refreshHours));

        if (!session.IsCoherent)
        {
            throw new InvalidOperationException(
                $"{JwtSectionName}:RefreshTokenExpirationInHours must be greater than 0, otherwise every refresh token is born expired.");
        }

        services.AddSingleton(session);

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IPasswordResetNotifier, SmtpPasswordResetNotifier>();
    }

    private static void AddWorkflowStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WorkflowStorageOptions>(configuration.GetSection(WorkflowStorageOptions.SectionName));
        services.AddSingleton<IWorkflowFileStorage, LocalWorkflowFileStorage>();

        services.Configure<WorkInstructionStorageOptions>(configuration.GetSection(WorkInstructionStorageOptions.SectionName));
        services.AddSingleton<IWorkInstructionFileStorage, LocalWorkInstructionFileStorage>();

        services.Configure<SupplierStorageOptions>(configuration.GetSection(SupplierStorageOptions.SectionName));
        services.AddSingleton<ISupplierFileStorage, LocalSupplierFileStorage>();
    }

    private static void AddMeOneHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var checks = services.AddHealthChecks();

        checks.AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag]);

        checks.Add(new HealthCheckRegistration(
            "meone-db",
            _ => new MeOneDatabaseHealthCheck(MeOneConnectionString(configuration)),
            HealthStatus.Unhealthy,
            tags: [ReadyTag],
            timeout: TimeSpan.FromSeconds(3)));
    }

    private static string MeOneConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MeOne");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:MeOne is not configured. It is the MeOne MySQL database (users, roles, " +
                "departments and refresh tokens).");
        }

        return connectionString;
    }
}
