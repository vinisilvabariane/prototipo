using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.Tests.API;

public sealed class HealthSurfaceGuardTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private static Type[] Controllers() => ApiAssembly
        .GetTypes()
        .Where(type => type is { IsAbstract: false, IsClass: true })
        .Where(type => type.GetCustomAttribute<ApiControllerAttribute>() is not null)
        .ToArray();

    [Fact]
    public void Scan_FindsControllers()
    {
        Assert.NotEmpty(Controllers());
    }

    [Fact]
    public void NoController_ExposesHealthRoute()
    {
        var offenders = Controllers()
            .Select(type => new
            {
                Name = type.Name,
                Route = type.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty
            })
            .Where(candidate =>
                candidate.Route.Contains("health", StringComparison.OrdinalIgnoreCase) ||
                candidate.Name.Contains("Health", StringComparison.OrdinalIgnoreCase))
            .Select(candidate => $"{candidate.Name} ({candidate.Route})")
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"Health is served by HealthEndpoints, not by a controller. Remove: {string.Join(", ", offenders)}");
    }
}
