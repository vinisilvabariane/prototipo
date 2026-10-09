using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IRevokeRoleUseCase
{
    Task<Result> ExecuteAsync(RevokeRoleRequest request, CancellationToken cancellationToken);
}
