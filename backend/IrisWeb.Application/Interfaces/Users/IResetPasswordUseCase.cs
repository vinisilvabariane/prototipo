using FluentResults;
using IrisWeb.Application.Communication.Users.Responses;

namespace IrisWeb.Application.Interfaces.Users;

public interface IResetPasswordUseCase
{
    Task<Result<ResetPasswordResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken);
}
