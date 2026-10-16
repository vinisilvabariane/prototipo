using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class ListProductionProductsUseCase(IProductionProductRepository products) : IListProductionProductsUseCase
{
    public async Task<Result<IReadOnlyList<ProductionProductResponse>>> ExecuteAsync(SetupQuery query, CancellationToken cancellationToken)
    {
        var found = await products.ListAsync(query.IncludeInactive, cancellationToken);

        return Result.Ok<IReadOnlyList<ProductionProductResponse>>([.. found.Select(ProductionMapping.ToResponse)]);
    }
}
