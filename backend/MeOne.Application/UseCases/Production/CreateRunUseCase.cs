using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CreateRunUseCase(
    IProductionRunRepository runs,
    ILineLayoutRepository layouts,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    ICurrentActor currentActor) : ICreateRunUseCase
{
    public async Task<Result<RunSummaryResponse>> ExecuteAsync(CreateRunRequest request, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<RunSummaryResponse>();

        var layout = await layouts.GetByIdAsync(request.LayoutId, cancellationToken);
        if (layout is null)
            return Result.Fail<RunSummaryResponse>(new NotFoundError(ProductionErrors.LayoutNotFound));

        var lookup = await ProductionLookups.FindAsync(lines, products, shifts, layout.LineId, layout.ProductId, layout.ShiftId, true, cancellationToken);
        if (lookup.IsFailed)
            return lookup.ToResult<RunSummaryResponse>();

        if (await runs.LineHasActiveRunAsync(layout.LineId, cancellationToken))
            return Result.Fail<RunSummaryResponse>(ProductionErrors.LineBusy(lookup.Value.Line.Name));

        var run = ProductionRun.Create(layout, actorId.Value, DateTime.UtcNow);
        await runs.AddAsync(run, cancellationToken);

        return await runs.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(ProductionMapping.ToSummary(run, lookup.Value.Line, lookup.Value.Product, lookup.Value.Shift))
            : Result.Fail<RunSummaryResponse>(ProductionErrors.NothingPersisted);
    }
}
