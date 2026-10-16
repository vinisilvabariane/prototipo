using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class ListProductionLinesUseCase(IProductionLineRepository lines) : IListProductionLinesUseCase
{
    public async Task<Result<IReadOnlyList<ProductionLineResponse>>> ExecuteAsync(SetupQuery query, CancellationToken cancellationToken)
    {
        var found = await lines.ListAsync(query.IncludeInactive, cancellationToken);

        return Result.Ok<IReadOnlyList<ProductionLineResponse>>([.. found.Select(ProductionMapping.ToResponse)]);
    }
}
