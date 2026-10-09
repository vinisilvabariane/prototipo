using FluentResults;
using IrisWeb.Application.Communication.Common.Responses;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Roles;

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
