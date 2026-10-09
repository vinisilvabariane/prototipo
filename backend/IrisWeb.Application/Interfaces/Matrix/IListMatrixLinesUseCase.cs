using FluentResults;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IListMatrixLinesUseCase
{
    Task<Result<IReadOnlyList<string>>> ExecuteAsync(CancellationToken cancellationToken);
}
