using FluentResults;
using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface IListUserRolesUseCase
{
    Task<Result<IReadOnlyList<RoleAssignmentResponse>>> ExecuteAsync(ulong userId, CancellationToken cancellationToken);
}
