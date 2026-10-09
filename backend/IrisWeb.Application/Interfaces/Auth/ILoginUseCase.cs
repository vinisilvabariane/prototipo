using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Communication.Auth.Responses;

namespace IrisWeb.Application.Interfaces.Auth;

public interface ILoginUseCase
{
    Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken);
}
