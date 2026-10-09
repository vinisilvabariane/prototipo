using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface ICreateUserUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken);
}
