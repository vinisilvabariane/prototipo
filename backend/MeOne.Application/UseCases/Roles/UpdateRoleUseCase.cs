using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

public sealed class UpdateRoleUseCase(IRoleRepository roles) : IUpdateRoleUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(id, cancellationToken);
        if (role is null)
            return Result.Fail(new NotFoundError(RoleErrors.NotFound));

        if (AccessRoles.IsMaster(role.Name) && !AccessRoles.IsMaster(request.Name))
            return Result.Fail(RoleErrors.MasterRoleProtected);

        var name = request.Name.Trim();

        if (await roles.NameExistsAsync(name, exceptRoleId: id, cancellationToken))
            return Result.Fail(RoleErrors.NameTaken(name));

        if (!role.Update(request.Name, request.Description))
            return Result.Ok();

        roles.Update(role);

        return await roles.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(RoleErrors.NothingPersisted);
    }
}
