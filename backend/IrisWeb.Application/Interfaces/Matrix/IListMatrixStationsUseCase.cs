using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IListMatrixStationsUseCase
{
    Task<Result<IReadOnlyList<MatrixStationResponse>>> ExecuteAsync(
        MatrixStationsQuery query,
        CancellationToken cancellationToken);
}
