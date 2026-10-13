using FluentResults;
using MeOne.Application.Communication.Users.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface IResetPasswordUseCase
{
    Task<Result<ResetPasswordResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken);
}
