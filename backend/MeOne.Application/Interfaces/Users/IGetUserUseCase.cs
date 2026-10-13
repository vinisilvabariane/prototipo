using FluentResults;
using MeOne.Application.Communication.Users.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface IGetUserUseCase
{
    Task<Result<UserResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
