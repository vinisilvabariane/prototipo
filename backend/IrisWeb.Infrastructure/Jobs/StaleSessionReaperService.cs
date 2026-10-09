using IrisWeb.Application.Interfaces.Stations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IrisWeb.Infrastructure.Jobs;

/// <summary>
/// Fecha sozinho presenca/vacancia/desconhecido que ficaram abertos para sempre porque o app que
/// devia fechar (web, mobile, desktop) morreu antes de avisar. Rede de seguranca: o caso comum ja
/// e coberto na hora, quando o posto volta a ser usado (ver OpenDevice*UseCase.CloseStrayOpenAsync)
/// — isto aqui so pega o posto que ninguem mais voltou a usar.
/// </summary>
public sealed class StaleSessionReaperService(
    IServiceScopeFactory scopeFactory,
    IOptions<StaleSessionReaperOptions> options,
    ILogger<StaleSessionReaperService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, options.Value.IntervalMinutes));
        var maxOpenAge = TimeSpan.FromHours(Math.Max(1, options.Value.MaxOpenHours));

        using var timer = new PeriodicTimer(interval);

        do
        {
            await RunOnceAsync(maxOpenAge, stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(TimeSpan maxOpenAge, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ICloseStaleSessionsUseCase>();
            var result = await useCase.ExecuteAsync(maxOpenAge, stoppingToken);

            if (result.IsSuccess && result.Value > 0)
            {
                logger.LogWarning(
                    "StaleSessionReaper fechou {Count} sessao(oes) de posto abertas ha mais de {MaxOpenHours}h sem encerramento.",
                    result.Value,
                    maxOpenAge.TotalHours);
            }
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(ex, "StaleSessionReaper falhou numa rodada; tenta de novo na proxima.");
        }
    }
}
