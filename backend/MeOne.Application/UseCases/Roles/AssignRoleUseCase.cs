using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Roles;

public sealed class AssignRoleUseCase(IUserRepository users, IRoleRepository roles) : IAssignRoleUseCase
{
    public async Task<Result> ExecuteAsync(AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var role = await roles.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            return Result.Fail(new NotFoundError(RoleErrors.NotFound));

        if (user.HasRole(role.Id))
            return Result.Fail(RoleErrors.AlreadyAssigned);

        user.GrantRole(role);
        users.Update(user);

        return await users.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(RoleErrors.NothingPersisted);
    }
}
