using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Communication.Users.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface ICreateUserUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken);
}
