using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Roles;

public sealed class UpdateRoleUseCase(IRoleRepository roles) : IUpdateRoleUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(id, cancellationToken);
        if (role is null)
            return Result.Fail(new NotFoundError(RoleErrors.NotFound));

        // Renomear a master derruba o [Authorize] de toda a governança na hora.
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
