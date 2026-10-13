using System.Reflection;
using System.Text.Json;
using MeOne.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeOne.API.Health;

public static class HealthEndpoints
{
    public const string CachePolicy = "health";

    private static readonly string Version =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";

    private static readonly Dictionary<HealthStatus, int> ResultStatusCodes = new()
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    };

    public static IEndpointRouteBuilder MapMeOneHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var health = endpoints.MapGroup("/health").WithTags("Health");

        var probes = health
            .MapGroup(string.Empty)
            .AllowAnonymous()
            .ExcludeFromDescription();

        probes.MapHealthChecks("live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(DependencyInjection.LiveTag),
            ResponseWriter = (_, _) => Task.CompletedTask
        });

        probes.MapHealthChecks("ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(DependencyInjection.ReadyTag),
            ResponseWriter = (_, _) => Task.CompletedTask,
            ResultStatusCodes = ResultStatusCodes
        })
        .CacheOutput(CachePolicy);
        health.MapHealthChecks(string.Empty, new HealthCheckOptions
        {
            ResponseWriter = WriteDetailAsync,
            ResultStatusCodes = ResultStatusCodes
        })
        .RequireAuthorization();
        return endpoints;
    }

    private static Task WriteDetailAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            service = "meone",
            version = Version,
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                tags = entry.Value.Tags
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
