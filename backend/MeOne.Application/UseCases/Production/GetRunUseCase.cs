using FluentResults;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Production;

public sealed class GetRunUseCase(
    IProductionRunRepository runs,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    IUserRepository users,
    ICurrentActor currentActor) : IGetRunUseCase
{
    public async Task<Result<RunDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var run = await ProductionLookups.OpenRunAsync(runs, id, cancellationToken);
        if (run.IsFailed)
            return run.ToResult<RunDetailResponse>();

        var lookup = await ProductionLookups.FindAsync(lines, products, shifts, run.Value.LineId, run.Value.ProductId, run.Value.ShiftId, false, cancellationToken);
        if (lookup.IsFailed)
            return lookup.ToResult<RunDetailResponse>();

        var names = await ProductionLookups.UsersAsync(users, ProductionMapping.UserIdsOf(run.Value), cancellationToken);
        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(ProductionMapping.ToDetail(
            run.Value,
            lookup.Value.Line,
            lookup.Value.Product,
            lookup.Value.Shift,
            names,
            DateTime.UtcNow,
            profile?.Allows(Permissions.ProductionSupervise) ?? false,
            profile?.Allows(Permissions.ProductionExecute) ?? false));
    }
}
