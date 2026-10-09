using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class ListMatrixProductsUseCase(ISyncMatrixClient sync) : IListMatrixProductsUseCase
{
    public async Task<Result<IReadOnlyList<MatrixProductResponse>>> ExecuteAsync(
        MatrixProductsQuery query,
        CancellationToken cancellationToken)
    {
        var line = query.Line!.Trim();

        var products = await MatrixMapping.GuardAsync(() => sync.ListConfiguredProductsAsync(line, cancellationToken));

        if (products.IsFailed)
            return products.ToResult();

        IReadOnlyList<MatrixProductResponse> response = [.. products.Value
            .Select(item => new MatrixProductResponse(item.Product, item.Shifts ?? []))];

        return Result.Ok(response);
    }
}
