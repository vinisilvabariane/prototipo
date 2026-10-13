using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

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

        if (currentActor.UserId == user.Id && link.Role is not null && AccessRoles.IsMaster(link.Role.Name))
            return Result.Fail(RoleErrors.CannotRevokeOwnMaster);

        user.RevokeRole(request.RoleId);
        users.Update(user);

        return await users.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(RoleErrors.NothingPersisted);
    }
}
