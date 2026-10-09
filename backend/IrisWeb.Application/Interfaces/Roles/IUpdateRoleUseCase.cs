using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IUpdateRoleUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateRoleRequest request, CancellationToken cancellationToken);
}
