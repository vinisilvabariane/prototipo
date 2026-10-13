using FluentResults;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Interfaces.Roles;

public interface ICreateRoleUseCase
{
    Task<Result<RoleResponse>> ExecuteAsync(CreateRoleRequest request, CancellationToken cancellationToken);
}
