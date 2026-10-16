using FluentResults;
using MeOne.Application.Communication.Users.Requests;

namespace MeOne.Application.Interfaces.Users;

public interface IConfirmPasswordResetUseCase
{
    Task<Result> ExecuteAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken);
}
