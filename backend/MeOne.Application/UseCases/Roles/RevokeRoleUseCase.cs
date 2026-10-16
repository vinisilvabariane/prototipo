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

public sealed class RevokeRoleUseCase(
    IUserRepository users,
    IScopeRepository scopes,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : IRevokeRoleUseCase
{
    public async Task<Result> ExecuteAsync(RevokeRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var user = await users.GetWithAccessAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var link = user.Roles.FirstOrDefault(item => item.RoleId == request.RoleId && item.ScopeId == request.ScopeId);
        if (link?.Role is null)
            return Result.Fail(RoleErrors.NotAssigned);

        if (link.Role.IsAdministrator)
        {
            if (actor.Value.UserId == user.Id)
                return Result.Fail(RoleErrors.CannotRevokeOwnMaster);

            var lastAdministrator = await AccessGuard.EnsureNotLastAdministratorAsync(users, user, cancellationToken);
            if (lastAdministrator.IsFailed)
                return lastAdministrator;
        }

        var chain = await RolesMapping.ScopeChainAsync(scopes, request.ScopeId, cancellationToken);
        if (chain.IsFailed)
            return chain.ToResult();

        var allowed = RolesMapping.EnsureCanHandle(actor.Value, link.Role, chain.Value);
        if (allowed.IsFailed)
            return allowed;

        user.RevokeRole(request.RoleId, request.ScopeId);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(RoleErrors.NothingPersisted);

        accessProfiles.Invalidate(user.Id);

        return Result.Ok();
    }
}
