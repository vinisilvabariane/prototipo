using FluentResults;
using IrisWeb.Application.Communication.Common.Responses;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface IListUsersUseCase
{
    Task<Result<PagedResponse<UserResponse>>> ExecuteAsync(UsersQuery query, CancellationToken cancellationToken);
}
