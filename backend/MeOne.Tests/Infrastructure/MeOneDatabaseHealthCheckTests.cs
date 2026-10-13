using MeOne.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MeOne.Tests.Infrastructure;

public sealed class MeOneDatabaseHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_UnreachableDatabase_ReturnsRegistrationFailureStatus()
    {

        var check = new MeOneDatabaseHealthCheck(
            "Server=localhost;Port=1;Database=nope;User=nope;Password=nope;Connection Timeout=2;");

        var result = await check.CheckHealthAsync(Context(HealthStatus.Degraded), CancellationToken.None);

        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Equal("MeOne database is unreachable.", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_FailureDoesNotThrow_HonorsUnhealthyFailureStatus()
    {
        var check = new MeOneDatabaseHealthCheck(
            "Server=localhost;Port=1;Database=nope;User=nope;Password=nope;Connection Timeout=2;");

        var result = await check.CheckHealthAsync(Context(HealthStatus.Unhealthy), CancellationToken.None);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.NotNull(result.Exception);
    }

    private static HealthCheckContext Context(HealthStatus failureStatus) => new()
    {
        Registration = new HealthCheckRegistration(
            "meone-db",
            _ => throw new NotSupportedException("Instance comes from the test."),
            failureStatus,
            tags: null)
    };
}
