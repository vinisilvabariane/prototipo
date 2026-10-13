using FluentResults;
using MeOne.Application.Communication.Roles.Requests;

namespace MeOne.Application.Interfaces.Roles;

public interface IUpdateRoleUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateRoleRequest request, CancellationToken cancellationToken);
}
