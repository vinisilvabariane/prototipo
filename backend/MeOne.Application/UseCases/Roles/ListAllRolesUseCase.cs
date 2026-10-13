using FluentResults;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Roles;

public sealed class ListAllRolesUseCase(IRoleRepository roles) : IListAllRolesUseCase
{
    public async Task<Result<IReadOnlyList<RoleResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await roles.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<RoleResponse>>([.. all.Select(RolesMapping.ToResponse)]);
    }
}
