using FluentResults;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Roles;

/// <summary>
/// Recusa role em uso: a FK de <c>user_roles</c> é cascade e apagaria as atribuições em silêncio.
/// </summary>
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
