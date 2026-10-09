using FluentResults;
using IrisWeb.Application.Communication.Users.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface IGetUserUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
