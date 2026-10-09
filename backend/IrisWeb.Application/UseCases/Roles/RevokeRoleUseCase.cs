using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Roles;

/// <summary>
/// A role some do JWT só quando o token atual expirar (minutos) ou o usuário renovar a sessão.
/// </summary>
public sealed class RevokeRoleUseCase(IUserRepository users, ICurrentActor currentActor) : IRevokeRoleUseCase
{
    public async Task<Result> ExecuteAsync(RevokeRoleRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var link = user.Roles.FirstOrDefault(item => item.RoleId == request.RoleId);
        if (link is null)
            return Result.Fail(RoleErrors.NotAssigned);

        // Sem essa trava o último master pode se trancar para fora da governança.
        if (currentActor.UserId == user.Id && link.Role is not null && AccessRoles.IsMaster(link.Role.Name))
            return Result.Fail(RoleErrors.CannotRevokeOwnMaster);

        user.RevokeRole(request.RoleId);
        users.Update(user);

        return await users.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(RoleErrors.NothingPersisted);
    }
}
