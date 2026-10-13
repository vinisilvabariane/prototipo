using FluentResults;
using MeOne.Application.Communication.Users.Requests;

namespace MeOne.Application.Interfaces.Users;

public interface IUpdateUserUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateUserRequest request, CancellationToken cancellationToken);
}
