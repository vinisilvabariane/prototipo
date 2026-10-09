using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IAllocateMatrixStationUseCase
{
    Task<Result<MatrixStationResponse>> ExecuteAsync(
        AllocateMatrixStationRequest request,
        CancellationToken cancellationToken);
}
