using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Production;

public sealed class ListRunsUseCase(
    IProductionRunRepository runs,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    ICurrentActor currentActor) : IListRunsUseCase
{
    public async Task<Result<RunPageResponse>> ExecuteAsync(RunsQuery query, CancellationToken cancellationToken)
    {
        var total = await runs.CountAsync(query.Status, query.LineId, cancellationToken);
        var found = await runs.SearchAsync(query.Status, query.LineId, query.Page * query.PageSize, query.PageSize, cancellationToken);
        var (lineMap, productMap, shiftMap) = await ProductionLookups.AllAsync(lines, products, shifts, cancellationToken);

        var items = found
            .Where(run => lineMap.ContainsKey(run.LineId) && productMap.ContainsKey(run.ProductId) && shiftMap.ContainsKey(run.ShiftId))
            .Select(run => ProductionMapping.ToSummary(run, lineMap[run.LineId], productMap[run.ProductId], shiftMap[run.ShiftId]))
            .ToList();

        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new RunPageResponse(items, total, query.Page, query.PageSize, profile?.Allows(Permissions.ProductionSupervise) ?? false));
    }
}
