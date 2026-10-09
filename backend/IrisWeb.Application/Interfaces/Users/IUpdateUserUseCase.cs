using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;

namespace IrisWeb.Application.Interfaces.Users;

public interface IUpdateUserUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateUserRequest request, CancellationToken cancellationToken);
}
