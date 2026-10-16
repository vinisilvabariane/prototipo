using FluentResults;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

public sealed class ListPermissionsUseCase : IListPermissionsUseCase
{
    public Task<Result<IReadOnlyList<PermissionResponse>>> ExecuteAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Result.Ok<IReadOnlyList<PermissionResponse>>(
            [.. Permissions.All.Select(permission =>
                new PermissionResponse(permission.Code, permission.Description, permission.GlobalOnly))]));
}
