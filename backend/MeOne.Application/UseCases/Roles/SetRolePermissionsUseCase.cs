using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Roles;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Roles;

public sealed class SetRolePermissionsUseCase(
    IRoleRepository roles,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : ISetRolePermissionsUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var role = await roles.GetWithPermissionsAsync(id, cancellationToken);
        if (role is null)
            return Result.Fail(new NotFoundError(RoleErrors.NotFound));

        if (role.IsAdministrator)
            return Result.Fail(RoleErrors.MasterRoleProtected);

        var permissions = RolesMapping.Normalize(request.Permissions);

        if (RolesMapping.UnknownPermission(permissions) is { } unknown)
            return Result.Fail(unknown);

        var touched = permissions.Except(role.PermissionCodes).Concat(role.PermissionCodes.Except(permissions));

        if (!actor.Value.Covers(touched, scopeChain: null))
            return Result.Fail(new ForbiddenError(RoleErrors.PermissionsBeyondOwn));

        if (!role.SetPermissions(permissions))
            return Result.Ok();

        roles.Update(role);

        if (await roles.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(RoleErrors.NothingPersisted);

        accessProfiles.InvalidateAll();

        return Result.Ok();
    }
}
