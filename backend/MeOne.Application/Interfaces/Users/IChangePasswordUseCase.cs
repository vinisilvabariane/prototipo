using FluentResults;
using MeOne.Application.Communication.Users.Requests;

namespace MeOne.Application.Interfaces.Users;

public interface IChangePasswordUseCase
{
    Task<Result> ExecuteAsync(ChangePasswordRequest request, CancellationToken cancellationToken);
}
