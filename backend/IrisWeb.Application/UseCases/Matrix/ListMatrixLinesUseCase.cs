using FluentResults;
using IrisWeb.Application.Interfaces.Matrix;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class ListMatrixLinesUseCase(ISyncMatrixClient sync) : IListMatrixLinesUseCase
{
    public async Task<Result<IReadOnlyList<string>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var lines = await MatrixMapping.GuardAsync(() => sync.ListLinesAsync(cancellationToken));

        if (lines.IsFailed)
            return lines.ToResult();

        IReadOnlyList<string> ordered = [.. lines.Value
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Order(StringComparer.OrdinalIgnoreCase)];

        return Result.Ok(ordered);
    }
}
