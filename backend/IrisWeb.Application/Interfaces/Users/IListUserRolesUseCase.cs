using FluentResults;
using IrisWeb.Application.Communication.Roles.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface IListUserRolesUseCase
{
    Task<Result<IReadOnlyList<RoleResponse>>> ExecuteAsync(ulong userId, CancellationToken cancellationToken);
}
