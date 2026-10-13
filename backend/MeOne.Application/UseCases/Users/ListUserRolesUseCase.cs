using FluentResults;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Users;
using MeOne.Application.UseCases.Roles;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class ListUserRolesUseCase(IUserRepository users) : IListUserRolesUseCase
{
    public async Task<Result<IReadOnlyList<RoleResponse>>> ExecuteAsync(ulong userId, CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessAsync(userId, cancellationToken);

        IReadOnlyList<RoleResponse> roles = user is null
            ? []
            : [.. user.Roles.Where(link => link.Role is not null).Select(link => RolesMapping.ToResponse(link.Role!))];

        return Result.Ok(roles);
    }
}
