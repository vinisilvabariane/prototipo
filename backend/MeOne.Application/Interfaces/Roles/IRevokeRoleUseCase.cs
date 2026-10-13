using FluentResults;
using MeOne.Application.Communication.Roles.Requests;

namespace MeOne.Application.Interfaces.Roles;

public interface IRevokeRoleUseCase
{
    Task<Result> ExecuteAsync(RevokeRoleRequest request, CancellationToken cancellationToken);
}
