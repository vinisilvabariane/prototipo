using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Roles;

/// <summary>A role nova só aparece no JWT no próximo login ou refresh do usuário.</summary>
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
