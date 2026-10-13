using FluentResults;
using MeOne.Application.Communication.Users.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface IGetUserByRegistrationUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken);
}
