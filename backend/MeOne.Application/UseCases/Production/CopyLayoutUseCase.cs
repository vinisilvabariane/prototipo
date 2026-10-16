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

public sealed class CopyLayoutUseCase(
    ILineLayoutRepository layouts,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    ICurrentActor currentActor) : ICopyLayoutUseCase
{
    private const int MaxTargets = 50;

    public async Task<Result<CopyLayoutResponse>> ExecuteAsync(ulong id, CopyLayoutRequest request, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<CopyLayoutResponse>();

        var source = await layouts.GetByIdAsync(id, cancellationToken);
        if (source is null)
            return Result.Fail<CopyLayoutResponse>(new NotFoundError(ProductionErrors.LayoutNotFound));

        if (request.Targets.Count == 0)
            return Result.Fail<CopyLayoutResponse>(ProductionErrors.CopyTargetsRequired);

        if (request.Targets.Count > MaxTargets)
            return Result.Fail<CopyLayoutResponse>(ProductionErrors.CopyTargetsTooMany);

        var keys = request.Targets.Select(target => (target.LineId, target.ProductId, target.ShiftId)).ToList();

        if (keys.Distinct().Count() != keys.Count)
            return Result.Fail<CopyLayoutResponse>(ProductionErrors.CopyTargetRepeated);

        if (keys.Contains((source.LineId, source.ProductId, source.ShiftId)))
            return Result.Fail<CopyLayoutResponse>(ProductionErrors.CopyToItself);

        var now = DateTime.UtcNow;
        var created = 0;
        var updated = 0;

        foreach (var target in request.Targets)
        {
            var lookup = await ProductionLookups.FindAsync(lines, products, shifts, target.LineId, target.ProductId, target.ShiftId, true, cancellationToken);
            if (lookup.IsFailed)
                return lookup.ToResult<CopyLayoutResponse>();

            var existing = await layouts.FindAsync(target.LineId, target.ProductId, target.ShiftId, cancellationToken);

            if (existing is not null)
            {
                existing.CopyContentFrom(source, actorId.Value, now);
                layouts.Update(existing);
                updated++;
                continue;
            }

            var copy = LineLayout.Create(target.LineId, target.ProductId, target.ShiftId, actorId.Value, now);
            copy.CopyContentFrom(source, actorId.Value, now);
            await layouts.AddAsync(copy, cancellationToken);
            created++;
        }

        return await layouts.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(new CopyLayoutResponse(created, updated))
            : Result.Fail<CopyLayoutResponse>(ProductionErrors.NothingPersisted);
    }
}
