using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Roles;

public sealed class CreateRoleUseCase(IRoleRepository roles) : ICreateRoleUseCase
{
    public async Task<Result<RoleResponse>> ExecuteAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = Role.Create(request.Name, request.Description);

        if (await roles.NameExistsAsync(role.Name, exceptRoleId: null, cancellationToken))
            return Result.Fail(RoleErrors.NameTaken(role.Name));

        await roles.AddAsync(role, cancellationToken);

        if (await roles.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(RoleErrors.NothingPersisted);

        return Result.Ok(RolesMapping.ToResponse(role));
    }
}
