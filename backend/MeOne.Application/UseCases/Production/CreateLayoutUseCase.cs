using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CreateLayoutUseCase(
    ILineLayoutRepository layouts,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    ICurrentActor currentActor) : ICreateLayoutUseCase
{
    public async Task<Result<LayoutSummaryResponse>> ExecuteAsync(CreateLayoutRequest request, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<LayoutSummaryResponse>();

        var lookup = await ProductionLookups.FindAsync(lines, products, shifts, request.LineId, request.ProductId, request.ShiftId, true, cancellationToken);
        if (lookup.IsFailed)
            return lookup.ToResult<LayoutSummaryResponse>();

        if (await layouts.FindAsync(request.LineId, request.ProductId, request.ShiftId, cancellationToken) is not null)
        {
            return Result.Fail<LayoutSummaryResponse>(
                ProductionErrors.LayoutTaken(lookup.Value.Line.Name, lookup.Value.Product.Code, lookup.Value.Shift.Name));
        }

        var layout = LineLayout.Create(request.LineId, request.ProductId, request.ShiftId, actorId.Value, DateTime.UtcNow);
        await layouts.AddAsync(layout, cancellationToken);

        return await layouts.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(ProductionMapping.ToSummary(layout, lookup.Value.Line, lookup.Value.Product, lookup.Value.Shift))
            : Result.Fail<LayoutSummaryResponse>(ProductionErrors.NothingPersisted);
    }
}
