using IrisWeb.Infrastructure.Storage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IrisWeb.Infrastructure.HealthChecks;

public sealed class ModelStorageHealthCheck(ModelStorageOptions options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(options.Path);

        if (!Directory.Exists(root))
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                $"Pasta de artefatos '{root}' nao encontrada."));
        }

        var probe = Path.Combine(root, $".health-{Guid.NewGuid():N}");

        try
        {
            using (File.Create(probe)) { }
            File.Delete(probe);

            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception exception)
        {
            return Task.FromResult(new HealthCheckResult(
                context.Registration.FailureStatus, $"Pasta de artefatos '{root}' nao aceita escrita.", exception));
        }
    }
}
