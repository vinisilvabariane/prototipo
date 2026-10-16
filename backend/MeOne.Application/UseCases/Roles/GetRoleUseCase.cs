using FluentResults;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Roles;

public sealed class GetRoleUseCase(IRoleRepository roles) : IGetRoleUseCase
{
    public async Task<Result<RoleResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var role = await roles.GetWithPermissionsAsync(id, cancellationToken);

        return role is null
            ? Result.Fail(new NotFoundError(RoleErrors.NotFound))
            : Result.Ok(RolesMapping.ToResponse(role));
    }
}
