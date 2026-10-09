using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Domain.Entities;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class OpenDevicePresenceUseCase(
    IDeviceMatrixBindingRepository bindings,
    IStationPresenceRepository presences,
    ISyncMatrixClient sync) : IOpenDevicePresenceUseCase
{
    /// <summary>
    /// Quanto a abertura da presenca espera o Sync para gravar nome, nivel e alocado. Passou
    /// disso a presenca abre sem o retrato: o tablet nao pode ficar parado esperando o Sync.
    /// </summary>
    public static readonly TimeSpan SnapshotBudget = TimeSpan.FromSeconds(3);

    public async Task<Result<OpenPresenceResponse>> ExecuteAsync(
        string deviceId,
        OpenDevicePresenceRequest request,
        CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var registration = (request.Registration ?? string.Empty).Trim();
        if (registration.Length == 0)
            return Result.Fail(StationErrors.RegistrationRequired);

        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);
        if (binding is null)
            return Result.Fail(MatrixErrors.DeviceNotBound);

        await CloseStrayOpenAsync(binding, cancellationToken);

        var presence = binding.OpenPresence(registration);
        await DescribeAsync(presence, binding, registration, cancellationToken);

        await presences.AddAsync(presence, cancellationToken);

        if (await presences.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(StationErrors.NothingPersisted);

        return Result.Ok(new OpenPresenceResponse(presence.Id, presence.Workstation, presence.Line));
    }

    /// <summary>
    /// Abrir presenca nunca deveria achar outra ja aberta no mesmo posto — mas acha, quando o
    /// app que devia fechar morreu sem avisar (aba fechada, processo morto, crash). Sem isso essa
    /// linha velha fica "ocupado" para sempre na Linha, mesmo sem ninguem de fato la. Fechar aqui,
    /// no proximo ciclo real de abertura, resolve o caso comum sem depender de nenhum job rodar a
    /// tempo nem de adivinhar duracao de turno.
    /// </summary>
    private async Task CloseStrayOpenAsync(DeviceMatrixBinding binding, CancellationToken cancellationToken)
    {
        var stray = await presences.ListOpenAsync(binding.PresenceWorkstation, binding.PresenceLine, cancellationToken);

        foreach (var old in stray)
        {
            if (old.Close(0))
                presences.Update(old);
        }
    }

    /// <summary>
    /// Retrato de quem sentou: nome, nivel na atividade do posto e quem o Sync tinha alocado.
    /// E o que a linha do tempo usa para dizer "treinado ou nao". Falha do Sync nao impede a
    /// presenca: sem o retrato ela aparece como "nivel nao registrado".
    /// </summary>
    private async Task DescribeAsync(
        StationPresence presence,
        DeviceMatrixBinding binding,
        string registration,
        CancellationToken cancellationToken)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(SnapshotBudget);

        try
        {
            var layout = await sync.GetLayoutAsync(binding.Line, binding.Product, binding.Shift, budget.Token);
            var station = MatrixMapping.FindStation(layout, binding.StationId);
            if (station is null) return;

            var allocated = station.CollaboratorRegistration?.ToString();
            var isAllocated = allocated is not null &&
                              MatrixMapping.NormalizeRegistration(allocated) == MatrixMapping.NormalizeRegistration(registration);

            if (isAllocated)
            {
                presence.Describe(station.CollaboratorName, station.CollaboratorLevel, allocated);
                return;
            }

            presence.Describe(null, null, allocated);

            if (station.ActivityId is not > 0 || !int.TryParse(registration, out var number)) return;

            var collaborator = await sync.GetCollaboratorForActivityAsync(
                number,
                station.ActivityId.Value,
                binding.Product,
                binding.Shift,
                station.Id,
                layout.Id,
                budget.Token);

            presence.Describe(collaborator.Name, collaborator.Level, allocated);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }
}
