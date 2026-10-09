using FluentResults;
using IrisWeb.Application.Communication.Roles.Responses;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IGetRoleUseCase
{
    Task<Result<RoleResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
