using FluentResults;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Production;

public sealed class GetLayoutUseCase(
    ILineLayoutRepository layouts,
    IProductionLineRepository lines,
    IProductionProductRepository products,
    IProductionShiftRepository shifts,
    IUserRepository users,
    ICurrentActor currentActor) : IGetLayoutUseCase
{
    public async Task<Result<LayoutDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var layout = await layouts.GetByIdAsync(id, cancellationToken);
        if (layout is null)
            return Result.Fail<LayoutDetailResponse>(new NotFoundError(ProductionErrors.LayoutNotFound));

        var lookup = await ProductionLookups.FindAsync(lines, products, shifts, layout.LineId, layout.ProductId, layout.ShiftId, false, cancellationToken);
        if (lookup.IsFailed)
            return lookup.ToResult<LayoutDetailResponse>();

        var names = await ProductionLookups.UsersAsync(users, [layout.UpdatedBy], cancellationToken);
        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new LayoutDetailResponse(
            layout.Id,
            lookup.Value.Line.Id,
            lookup.Value.Line.Name,
            lookup.Value.Product.Id,
            lookup.Value.Product.Code,
            lookup.Value.Product.Name,
            lookup.Value.Shift.Id,
            lookup.Value.Shift.Name,
            [.. layout.Posts.OrderBy(post => post.Id).Select(ProductionMapping.ToResponse)],
            [.. layout.Decorations.OrderBy(decoration => decoration.Id).Select(ProductionMapping.ToResponse)],
            layout.UpdatedAt,
            names.TryGetValue(layout.UpdatedBy, out var author) ? author.Name : string.Empty,
            profile?.Allows(Permissions.EngineeringManage) ?? false));
    }
}
