using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

public sealed class DeleteRoleUseCase(IRoleRepository roles) : IDeleteRoleUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var role = await roles.GetByIdAsync(id, cancellationToken);
        if (role is null)
            return Result.Fail(new NotFoundError(RoleErrors.NotFound));

        if (AccessRoles.IsMaster(role.Name))
            return Result.Fail(RoleErrors.MasterRoleProtected);

        if (await roles.InUseAsync(id, cancellationToken))
            return Result.Fail(RoleErrors.InUse);

        roles.Delete(role);

        return await roles.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(RoleErrors.NothingPersisted);
    }
}
