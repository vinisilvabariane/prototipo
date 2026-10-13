using FluentResults;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Roles;

public sealed class ListRolesUseCase(IRoleRepository roles) : IListRolesUseCase
{
    public async Task<Result<PagedResponse<RoleResponse>>> ExecuteAsync(
        RolesQuery query,
        CancellationToken cancellationToken)
    {
        var total = await roles.CountAsync(cancellationToken);
        var page = await roles.ListPageAsync(query.Page * query.PageSize, query.PageSize, cancellationToken);

        return Result.Ok(new PagedResponse<RoleResponse>(
            [.. page.Select(RolesMapping.ToResponse)],
            total,
            query.Page,
            query.PageSize));
    }
}
