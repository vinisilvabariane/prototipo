using FluentResults;
using MeOne.Application.Communication.Auth.Requests;

namespace MeOne.Application.Interfaces.Auth;

public interface ILogoutUseCase
{
    Task<Result> ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken);
}
