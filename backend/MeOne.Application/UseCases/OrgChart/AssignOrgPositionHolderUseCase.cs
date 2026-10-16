using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class AssignOrgPositionHolderUseCase(IOrgPositionRepository positions, IUserRepository users)
    : IAssignOrgPositionHolderUseCase
{
    public async Task<Result> ExecuteAsync(
        ulong id,
        AssignOrgPositionHolderRequest request,
        CancellationToken cancellationToken)
    {
        var position = await positions.GetByIdAsync(id, cancellationToken);
        if (position is null)
            return Result.Fail(new NotFoundError(OrgChartErrors.NotFound));

        if (request.UserId is { } userId)
        {
            var found = await users.GetActiveByIdsAsync([userId], cancellationToken);
            if (found.Count == 0)
                return Result.Fail(new NotFoundError(OrgChartErrors.HolderNotFound));

            var all = await positions.GetAllAsync(cancellationToken);
            if (all.FirstOrDefault(other => other.Id != id && other.HolderId == userId) is { } occupied)
                return Result.Fail(OrgChartErrors.HolderTaken(occupied.Title));
        }

        if (!position.AssignHolder(request.UserId))
            return Result.Ok();

        positions.Update(position);

        return await positions.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OrgChartErrors.NothingPersisted);
    }
}
