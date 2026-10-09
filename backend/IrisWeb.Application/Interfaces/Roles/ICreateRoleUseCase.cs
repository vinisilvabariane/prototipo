using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Communication.Roles.Responses;

namespace IrisWeb.Application.Interfaces.Roles;

public interface ICreateRoleUseCase
{
    Task<Result<RoleResponse>> ExecuteAsync(CreateRoleRequest request, CancellationToken cancellationToken);
}
