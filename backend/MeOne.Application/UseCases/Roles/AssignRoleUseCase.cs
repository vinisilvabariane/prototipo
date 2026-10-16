using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Roles;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

public sealed class AssignRoleUseCase(
    IUserRepository users,
    IRoleRepository roles,
    IScopeRepository scopes,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : IAssignRoleUseCase
{
    public async Task<Result> ExecuteAsync(AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var user = await users.GetWithAccessAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var role = await roles.GetWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
            return Result.Fail(new NotFoundError(RoleErrors.NotFound));

        if (request.ScopeId is not null)
        {
            if (role.IsAdministrator)
                return Result.Fail(RoleErrors.MasterMustBeGlobal);

            if (role.PermissionCodes.Any(Permissions.IsGlobalOnly))
                return Result.Fail(RoleErrors.GlobalOnlyPermissionInScope);
        }

        var scope = request.ScopeId is { } scopeId ? await scopes.GetByIdAsync(scopeId, cancellationToken) : null;
        if (request.ScopeId is not null && scope is null)
            return Result.Fail(new NotFoundError(ScopeErrors.NotFound));

        var chain = await RolesMapping.ScopeChainAsync(scopes, request.ScopeId, cancellationToken);
        if (chain.IsFailed)
            return chain.ToResult();

        var allowed = RolesMapping.EnsureCanHandle(actor.Value, role, chain.Value);
        if (allowed.IsFailed)
            return allowed;

        if (user.HasRole(role.Id, request.ScopeId))
            return Result.Fail(RoleErrors.AlreadyAssigned);

        user.GrantRole(role, scope);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(RoleErrors.NothingPersisted);

        accessProfiles.Invalidate(user.Id);

        return Result.Ok();
    }
}
