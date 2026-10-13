using FluentResults;
using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Interfaces.Roles;

public interface IGetRoleUseCase
{
    Task<Result<RoleResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
