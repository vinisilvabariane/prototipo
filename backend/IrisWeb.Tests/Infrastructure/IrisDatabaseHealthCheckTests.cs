using IrisWeb.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IrisWeb.Tests.Infrastructure;

public sealed class IrisDatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_BaseInalcancavel_RetornaFailureStatusDoRegistro()
    {

        var check = new IrisDatabaseHealthCheck(
            "Server=localhost;Port=1;Database=nope;User=nope;Password=nope;Connection Timeout=2;");

        var result = await check.CheckHealthAsync(Context(HealthStatus.Degraded), CancellationToken.None);

        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Equal("Base Iris inacessivel.", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_FalhaNaoVazaExcecao_RespeitaFailureStatusUnhealthy()
    {
        var check = new IrisDatabaseHealthCheck(
            "Server=localhost;Port=1;Database=nope;User=nope;Password=nope;Connection Timeout=2;");

        var result = await check.CheckHealthAsync(Context(HealthStatus.Unhealthy), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.NotNull(result.Exception);
    }

    private static HealthCheckContext Context(HealthStatus failureStatus) => new()
    {
        Registration = new HealthCheckRegistration(
            "iris-db",
            _ => throw new NotSupportedException("Instancia vem do teste."),
            failureStatus,
            tags: null)
    };
}
