using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace IrisWeb.Tests.API;

public sealed class HealthSurfaceGuardTests
{
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;

    private static Type[] Controllers() => ApiAssembly
        .GetTypes()
        .Where(type => type is { IsAbstract: false, IsClass: true })
        .Where(type => type.GetCustomAttribute<ApiControllerAttribute>() is not null)
        .ToArray();

    [Fact]
    public void A_varredura_enxerga_os_controllers()
    {
        Assert.NotEmpty(Controllers());
    }

    [Fact]
    public void Nenhum_controller_expoe_rota_de_health()
    {
        var ofensores = Controllers()
            .Select(type => new
            {
                Nome = type.Name,
                Rota = type.GetCustomAttribute<RouteAttribute>()?.Template ?? string.Empty
            })
            .Where(candidato =>
                candidato.Rota.Contains("health", StringComparison.OrdinalIgnoreCase) ||
                candidato.Nome.Contains("Health", StringComparison.OrdinalIgnoreCase))
            .Select(candidato => $"{candidato.Nome} ({candidato.Rota})")
            .ToArray();

        Assert.True(
            ofensores.Length == 0,
            $"Health e servido por HealthEndpoints, nao por controller. Remova: {string.Join(", ", ofensores)}");
    }
}
