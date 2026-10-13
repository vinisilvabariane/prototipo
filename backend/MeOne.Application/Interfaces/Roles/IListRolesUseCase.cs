using FluentResults;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Interfaces.Roles;

public interface IListRolesUseCase
{
    Task<Result<PagedResponse<RoleResponse>>> ExecuteAsync(RolesQuery query, CancellationToken cancellationToken);
}
