using FluentResults;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Roles;

public sealed class GetRoleUseCase(IRoleRepository roles) : IGetRoleUseCase
{
    public async Task<Result<RoleResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(id, cancellationToken);

        return role is null
            ? Result.Fail(new NotFoundError(RoleErrors.NotFound))
            : Result.Ok(RolesMapping.ToResponse(role));
    }
}
