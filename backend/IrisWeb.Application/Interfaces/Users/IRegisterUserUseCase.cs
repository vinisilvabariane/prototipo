using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface IRegisterUserUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}
