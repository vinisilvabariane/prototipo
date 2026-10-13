using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Roles;

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
