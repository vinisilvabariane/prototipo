using FluentResults;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.UseCases.Matrix;

/// <summary>
/// "Liberar posto selecionado" do Sync: tira o operador so deste posto. Quem esta alocado sai
/// do layout relido na hora, e o posto vai sempre junto, porque sem ele o Sync libera todos
/// os postos do operador no layout.
/// </summary>
public sealed class ReleaseMatrixStationUseCase(ISyncMatrixClient sync) : IReleaseMatrixStationUseCase
{
    public async Task<Result<MatrixStationResponse>> ExecuteAsync(
        int stationId,
        MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var line = query.Line!.Trim();
        var product = query.Product!.Trim();
        var shift = query.Shift!.Trim();

        var layout = await MatrixMapping.GuardAsync(() => sync.GetLayoutAsync(line, product, shift, cancellationToken));
        if (layout.IsFailed)
            return layout.ToResult();

        var station = MatrixMapping.FindStation(layout.Value, stationId);
        if (station is null)
            return Result.Fail(MatrixErrors.StationNotInLayout(stationId, line, product, shift));

        if (station.CollaboratorId is not > 0)
            return Result.Fail(MatrixErrors.StationAlreadyEmpty(station.Id));

        var removal = new SyncRemoveCollaborator(station.CollaboratorId.Value, layout.Value.Id, station.Id);

        var removed = await MatrixMapping.GuardAsync(async () =>
        {
            await sync.RemoveCollaboratorAsync(removal, cancellationToken);
            return true;
        });
        if (removed.IsFailed)
            return removed.ToResult();

        var refreshed = await MatrixMapping.GuardAsync(() => sync.GetLayoutAsync(line, product, shift, cancellationToken));
        if (refreshed.IsFailed)
            return refreshed.ToResult();

        var updated = MatrixMapping.FindStation(refreshed.Value, station.Id) ?? station with
        {
            CollaboratorId = null,
            CollaboratorName = null,
            CollaboratorRegistration = null,
            CollaboratorLevel = null,
        };

        return Result.Ok(MatrixMapping.ToStation(updated));
    }
}
