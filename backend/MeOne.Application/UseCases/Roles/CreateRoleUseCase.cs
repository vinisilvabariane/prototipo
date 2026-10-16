using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Roles;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

public sealed class CreateRoleUseCase(IRoleRepository roles, ICurrentActor currentActor) : ICreateRoleUseCase
{
    public async Task<Result<RoleResponse>> ExecuteAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        if (AccessRoles.IsMaster(request.Name))
            return Result.Fail(RoleErrors.MasterRoleProtected);

        var permissions = RolesMapping.Normalize(request.Permissions);

        if (RolesMapping.UnknownPermission(permissions) is { } unknown)
            return Result.Fail(unknown);

        if (!actor.Value.Covers(permissions, scopeChain: null))
            return Result.Fail(new ForbiddenError(RoleErrors.PermissionsBeyondOwn));

        var role = Role.Create(request.Name, request.Description, permissions);

        if (await roles.NameExistsAsync(role.Name, exceptRoleId: null, cancellationToken))
            return Result.Fail(RoleErrors.NameTaken(role.Name));

        await roles.AddAsync(role, cancellationToken);

        if (await roles.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(RoleErrors.NothingPersisted);

        return Result.Ok(RolesMapping.ToResponse(role));
    }
}
