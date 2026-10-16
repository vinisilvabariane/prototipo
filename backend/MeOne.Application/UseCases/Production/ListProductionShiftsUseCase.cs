using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class ListProductionShiftsUseCase(IProductionShiftRepository shifts) : IListProductionShiftsUseCase
{
    public async Task<Result<IReadOnlyList<ProductionShiftResponse>>> ExecuteAsync(SetupQuery query, CancellationToken cancellationToken)
    {
        var found = await shifts.ListAsync(query.IncludeInactive, cancellationToken);

        return Result.Ok<IReadOnlyList<ProductionShiftResponse>>([.. found.Select(ProductionMapping.ToResponse)]);
    }
}
