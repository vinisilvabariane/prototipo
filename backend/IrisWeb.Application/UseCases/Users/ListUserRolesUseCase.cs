using FluentResults;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Application.UseCases.Roles;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

/// <summary>Usuário inexistente ou desativado devolve lista vazia, como no Access Control.</summary>
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
