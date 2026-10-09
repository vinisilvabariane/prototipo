using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class ListMatrixStationsUseCase(ISyncMatrixClient sync) : IListMatrixStationsUseCase
{
    public async Task<Result<IReadOnlyList<MatrixStationResponse>>> ExecuteAsync(
        MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var layout = await MatrixMapping.GuardAsync(() =>
            sync.GetLayoutAsync(query.Line!.Trim(), query.Product!.Trim(), query.Shift!.Trim(), cancellationToken));

        if (layout.IsFailed)
            return layout.ToResult();

        IReadOnlyList<MatrixStationResponse> stations = [.. (layout.Value.Configuration ?? [])
            .OrderBy(station => station.Id)
            .Select(MatrixMapping.ToStation)];

        return Result.Ok(stations);
    }
}
