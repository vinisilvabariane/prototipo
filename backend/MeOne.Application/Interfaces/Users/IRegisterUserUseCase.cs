using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Communication.Users.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface IRegisterUserUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken);
}
