using FluentResults;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Roles;

public sealed class ListAllRolesUseCase(IRoleRepository roles) : IListAllRolesUseCase
{
    public async Task<Result<IReadOnlyList<RoleResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await roles.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<RoleResponse>>([.. all.Select(RolesMapping.ToResponse)]);
    }
}
