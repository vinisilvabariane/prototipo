using FluentResults;
using MeOne.Application.Communication.Roles.Requests;

namespace MeOne.Application.Interfaces.Roles;

public interface IAssignRoleUseCase
{
    Task<Result> ExecuteAsync(AssignRoleRequest request, CancellationToken cancellationToken);
}
