using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class MoveOrgPositionUseCase(IOrgPositionRepository positions) : IMoveOrgPositionUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, MoveOrgPositionRequest request, CancellationToken cancellationToken)
    {
        var position = await positions.GetByIdAsync(id, cancellationToken);
        if (position is null)
            return Result.Fail(new NotFoundError(OrgChartErrors.NotFound));

        var all = await positions.GetAllAsync(cancellationToken);

        if (request.ParentId is { } parentId)
        {
            if (all.All(other => other.Id != parentId))
                return Result.Fail(new NotFoundError(OrgChartErrors.ParentNotFound));

            if (OrgChartHierarchy.IsSelfOrBelow(all, id, parentId))
                return Result.Fail(OrgChartErrors.MoveUnderItself);

            if (OrgChartHierarchy.DepthOf(all, parentId) + OrgChartHierarchy.HeightOf(all, id) > OrgChartHierarchy.MaxDepth)
                return Result.Fail(OrgChartErrors.TooDeep);
        }

        if (!position.MoveTo(request.ParentId))
            return Result.Ok();

        positions.Update(position);

        return await positions.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OrgChartErrors.NothingPersisted);
    }
}
