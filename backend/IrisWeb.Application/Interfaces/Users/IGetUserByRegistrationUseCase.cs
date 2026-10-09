using FluentResults;
using IrisWeb.Application.Communication.Users.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface IGetUserByRegistrationUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken);
}
