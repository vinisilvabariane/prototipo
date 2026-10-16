using FluentResults;
using MeOne.Application.Communication.OrgChart.Responses;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class ListOrgAssignableUsersUseCase(IOrgPositionRepository positions, IUserRepository users)
    : IListOrgAssignableUsersUseCase
{
    public async Task<Result<IReadOnlyList<OrgUserResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var occupied = (await positions.GetAllAsync(cancellationToken))
            .Where(position => position.HolderId is not null)
            .Select(position => position.HolderId!.Value)
            .ToHashSet();

        var free = (await users.ListActiveAsync(cancellationToken)).Where(user => !occupied.Contains(user.Id));

        return Result.Ok<IReadOnlyList<OrgUserResponse>>(
            [.. free.Select(user => new OrgUserResponse(user.Id, user.Registration, user.Name, user.Department?.Name ?? string.Empty))]);
    }
}
