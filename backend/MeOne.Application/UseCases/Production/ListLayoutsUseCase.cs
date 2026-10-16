using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Production;

public sealed class ListLayoutsUseCase(
    ILineLayoutRepository layouts,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    ICurrentActor currentActor) : IListLayoutsUseCase
{
    public async Task<Result<LayoutListResponse>> ExecuteAsync(LayoutsQuery query, CancellationToken cancellationToken)
    {
        var found = await layouts.ListAsync(query.LineId, query.ProductId, query.ShiftId, cancellationToken);
        var (lineMap, productMap, shiftMap) = await ProductionLookups.AllAsync(lines, products, shifts, cancellationToken);

        var items = found
            .Where(layout => lineMap.ContainsKey(layout.LineId) && productMap.ContainsKey(layout.ProductId) && shiftMap.ContainsKey(layout.ShiftId))
            .Select(layout => ProductionMapping.ToSummary(layout, lineMap[layout.LineId], productMap[layout.ProductId], shiftMap[layout.ShiftId]))
            .OrderBy(item => item.LineName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.ProductCode, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.ShiftName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new LayoutListResponse(items, profile?.Allows(Permissions.EngineeringManage) ?? false));
    }
}
