using FluentResults;
using IrisWeb.Application.Communication.Roles.Responses;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IListAllRolesUseCase
{
    Task<Result<IReadOnlyList<RoleResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
