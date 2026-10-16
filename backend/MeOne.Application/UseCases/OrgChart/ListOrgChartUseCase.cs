using FluentResults;
using MeOne.Application.Communication.OrgChart.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class ListOrgChartUseCase(
    IOrgPositionRepository positions,
    IDepartmentRepository departments,
    IUserRepository users,
    ICurrentActor currentActor) : IListOrgChartUseCase
{
    public async Task<Result<OrgChartResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await positions.GetAllAsync(cancellationToken);
        var departmentNames = (await departments.ListOrderedAsync(cancellationToken))
            .ToDictionary(department => department.Id, department => department.Name);

        var holderIds = all.Where(position => position.HolderId is not null).Select(position => position.HolderId!.Value).ToList();
        var holders = (await users.GetByIdsIncludingInactiveAsync(holderIds, cancellationToken))
            .ToDictionary(user => user.Id);

        var responses = all
            .OrderBy(position => position.Title, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(position => position.Id)
            .Select(position => OrgChartMapping.ToResponse(
                position,
                departmentNames.GetValueOrDefault(position.DepartmentId, string.Empty),
                position.HolderId is { } holderId ? holders.GetValueOrDefault(holderId) : null))
            .ToList();

        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new OrgChartResponse(responses, profile?.HasGlobal(Permissions.OrgChartManage) ?? false));
    }
}
