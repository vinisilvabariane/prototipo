using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

internal sealed record ProductionLookup(ProductionLine Line, ProductionProduct Product, ProductionShift Shift);

internal static class ProductionLookups
{
    public static async Task<Result<ProductionLookup>> FindAsync(
        IProductionLineRepository lines,
        IProductionProductRepository products,
        IProductionShiftRepository shifts,
        ulong lineId,
        ulong productId,
        ulong shiftId,
        bool requireActive,
        CancellationToken cancellationToken)
    {
        var line = await lines.GetByIdAsync(lineId, cancellationToken);
        if (line is null)
            return Result.Fail<ProductionLookup>(new NotFoundError(ProductionErrors.LineNotFound));

        var product = await products.GetByIdAsync(productId, cancellationToken);
        if (product is null)
            return Result.Fail<ProductionLookup>(new NotFoundError(ProductionErrors.ProductNotFound));

        var shift = await shifts.GetByIdAsync(shiftId, cancellationToken);
        if (shift is null)
            return Result.Fail<ProductionLookup>(new NotFoundError(ProductionErrors.ShiftNotFound));

        if (requireActive)
        {
            if (!line.Active)
                return Result.Fail<ProductionLookup>(ProductionErrors.LineInactive);

            if (!product.Active)
                return Result.Fail<ProductionLookup>(ProductionErrors.ProductInactive);

            if (!shift.Active)
                return Result.Fail<ProductionLookup>(ProductionErrors.ShiftInactive);
        }

        return Result.Ok(new ProductionLookup(line, product, shift));
    }

    public static async Task<(Dictionary<ulong, ProductionLine> Lines, Dictionary<ulong, ProductionProduct> Products, Dictionary<ulong, ProductionShift> Shifts)> AllAsync(
        IProductionLineRepository lines,
        IProductionProductRepository products,
        IProductionShiftRepository shifts,
        CancellationToken cancellationToken) =>
    (
        (await lines.ListAsync(true, cancellationToken)).ToDictionary(item => item.Id),
        (await products.ListAsync(true, cancellationToken)).ToDictionary(item => item.Id),
        (await shifts.ListAsync(true, cancellationToken)).ToDictionary(item => item.Id)
    );

    public static async Task<Dictionary<ulong, User>> UsersAsync(
        IUserRepository users,
        IEnumerable<ulong> ids,
        CancellationToken cancellationToken)
    {
        var distinct = ids.Distinct().ToList();

        return distinct.Count == 0
            ? []
            : (await users.GetByIdsIncludingInactiveAsync(distinct, cancellationToken)).ToDictionary(user => user.Id);
    }

    public static async Task<Result<ProductionRun>> OpenRunAsync(
        IProductionRunRepository runs,
        ulong runId,
        CancellationToken cancellationToken)
    {
        var run = await runs.GetByIdAsync(runId, cancellationToken);

        return run is null
            ? Result.Fail<ProductionRun>(new NotFoundError(ProductionErrors.RunNotFound))
            : Result.Ok(run);
    }

    public static async Task<Result<ulong>> ActorIdAsync(ICurrentActor currentActor, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);

        return actor.IsFailed ? actor.ToResult<ulong>() : Result.Ok(actor.Value.UserId);
    }
}
