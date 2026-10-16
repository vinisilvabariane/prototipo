using FluentResults;
using MeOne.Application.Communication.Roles.Requests;

namespace MeOne.Application.Interfaces.Roles;

public interface ISetRolePermissionsUseCase
{
    Task<Result> ExecuteAsync(ulong id, SetRolePermissionsRequest request, CancellationToken cancellationToken);
}
