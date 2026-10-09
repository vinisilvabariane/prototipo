using FluentResults;
using IrisWeb.Application.Communication.Common.Responses;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Communication.Roles.Responses;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IListRolesUseCase
{
    Task<Result<PagedResponse<RoleResponse>>> ExecuteAsync(RolesQuery query, CancellationToken cancellationToken);
}
