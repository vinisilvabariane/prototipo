using FluentResults;
using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Interfaces.Roles;

public interface IListPermissionsUseCase
{
    Task<Result<IReadOnlyList<PermissionResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
