using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IListMatrixProductsUseCase
{
    Task<Result<IReadOnlyList<MatrixProductResponse>>> ExecuteAsync(
        MatrixProductsQuery query,
        CancellationToken cancellationToken);
}
