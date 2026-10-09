using FluentResults;
using IrisWeb.Application.Communication.Roles.Requests;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IAssignRoleUseCase
{
    Task<Result> ExecuteAsync(AssignRoleRequest request, CancellationToken cancellationToken);
}
