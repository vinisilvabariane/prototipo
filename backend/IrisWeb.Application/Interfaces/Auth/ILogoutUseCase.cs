using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;

namespace IrisWeb.Application.Interfaces.Auth;

public interface ILogoutUseCase
{
    Task<Result> ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken);
}
