using FluentResults;
using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Interfaces.Roles;

public interface IListAllRolesUseCase
{
    Task<Result<IReadOnlyList<RoleResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
