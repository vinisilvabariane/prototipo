using FluentResults;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class ListUserRolesUseCase(IUserRepository users) : IListUserRolesUseCase
{
    public async Task<Result<IReadOnlyList<RoleAssignmentResponse>>> ExecuteAsync(
        ulong userId,
        CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessAsync(userId, cancellationToken);

        IReadOnlyList<RoleAssignmentResponse> assignments = user is null
            ? []
            : [.. user.Roles.Where(link => link.Role is not null).Select(UsersMapping.ToAssignment)];

        return Result.Ok(assignments);
    }
}
