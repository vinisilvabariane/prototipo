using IrisWeb.Domain.Rules;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Infrastructure.Email;
using IrisWeb.Infrastructure.Identity;
using IrisWeb.Infrastructure.Data.Context;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Infrastructure.Export;
using IrisWeb.Infrastructure.HealthChecks;
using IrisWeb.Infrastructure.Jobs;
using IrisWeb.Infrastructure.Storage;
using IrisWeb.Infrastructure.Sync;
using IrisWeb.Application.Interfaces.Matrix;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Infrastructure;

public static class DependencyInjection
{

    public const string LiveTag = "live";

    public const string ReadyTag = "ready";

    private const string FaceMatchingSectionName = "FaceRecognition";

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
        services.AddIrisDatabase(configuration);
        services.AddIdentity(configuration);
        services.AddSyncMatrix(configuration);
        services.AddModelArtifactStorage(configuration);
        services.AddFaceMatchingPolicy(configuration);
        services.AddIrisHealthChecks(configuration);
        services.AddStaleSessionReaper(configuration);
        services.AddSingleton<IStationExportWriter, ClosedXmlStationExportWriter>();

        return services;
    }

    private static void AddStaleSessionReaper(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StaleSessionReaperOptions>(configuration.GetSection(StaleSessionReaperOptions.SectionName));
        services.AddHostedService<StaleSessionReaperService>();
    }

    private static void AddIrisDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = IrisConnectionString(configuration);

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        services.AddDbContext<IrisWebDbContext>(options => options.UseMySql(connectionString, serverVersion));
    }

    private static void AddFaceMatchingPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(FaceMatchingSectionName);
        var fallback = FaceMatchingPolicy.Default;

        var policy = new FaceMatchingPolicy(
            section[nameof(FaceMatchingPolicy.ModelVersion)] is { Length: > 0 } modelVersion
                ? modelVersion
                : fallback.ModelVersion,
            section.GetValue("MatchTopK", fallback.MatchTopK),
            section.GetValue("DistanceThreshold", fallback.DistanceThreshold),
            section.GetValue("DistanceMargin", fallback.DistanceMargin));

        if (!policy.IsCoherent)
        {
            throw new InvalidOperationException(
                $"Secao {FaceMatchingSectionName} invalida: DistanceThreshold tem que ser > 0, " +
                "DistanceMargin >= 0, MatchTopK >= 1 e ModelVersion nao pode ser vazio. Com " +
                "DistanceThreshold 0 nenhum rosto casa e todo mundo vira desconhecido.");
        }

        services.AddSingleton(policy);

        var enrollment = section.GetSection("Enrollment");
        var enrollmentFallback = EnrollmentScreeningPolicy.Default;

        var screening = new EnrollmentScreeningPolicy(
            enrollment.GetValue("SamePersonDistance", enrollmentFallback.SamePersonDistance),
            enrollment.GetValue("SimilarDistance", enrollmentFallback.SimilarDistance),
            enrollment.GetValue("MaxSpread", enrollmentFallback.MaxSpread));

        if (!screening.IsCoherent)
        {
            throw new InvalidOperationException(
                $"Secao {FaceMatchingSectionName}:Enrollment invalida: SamePersonDistance tem que ser > 0, " +
                "SimilarDistance >= SamePersonDistance e MaxSpread > 0.");
        }

        services.AddSingleton(screening);
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
                $"{JwtSectionName}:RefreshTokenExpirationInHours tem que ser > 0, senao todo refresh nasce vencido.");
        }

        services.AddSingleton(session);

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IPasswordResetNotifier, SmtpPasswordResetNotifier>();
    }

    private static void AddSyncMatrix(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));

        services.AddHttpClient<ISyncMatrixClient, SyncMatrixClient>((provider, http) =>
        {
            var options = provider.GetRequiredService<IOptions<SyncOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException(
                    "Sync:BaseUrl nao configurado. Sem ele o tablet nao consegue conferir a alocacao da matriz.");
            }

            http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });
    }

    private static void AddModelArtifactStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(ModelStorageOptions.SectionName).Get<ModelStorageOptions>()
            ?? new ModelStorageOptions();

        services.AddSingleton(options);
        services.AddSingleton<IModelArtifactStorage>(new FileSystemModelArtifactStorage(options));
    }

    private static void AddIrisHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var checks = services.AddHealthChecks();

        checks.AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag]);

        checks.Add(new HealthCheckRegistration(
            "iris-db",
            _ => new IrisDatabaseHealthCheck(IrisConnectionString(configuration)),
            HealthStatus.Unhealthy,
            tags: [ReadyTag],
            timeout: TimeSpan.FromSeconds(3)));

        checks.Add(new HealthCheckRegistration(
            "model-storage",
            provider => new ModelStorageHealthCheck(provider.GetRequiredService<ModelStorageOptions>()),
            HealthStatus.Degraded,
            tags: [ReadyTag],
            timeout: TimeSpan.FromSeconds(3)));
    }

    private static string IrisConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Iris");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Iris nao configurado. E o MySQL do Iris, o mesmo banco que o " +
                "app desktop usa (variaveis IRIS_DB_* do .env dele).");
        }

        return connectionString;
    }
}
