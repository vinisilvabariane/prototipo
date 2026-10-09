using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Communication.Auth.Responses;

namespace IrisWeb.Application.Interfaces.Auth;

public interface IRefreshSessionUseCase
{
    Task<Result<AuthResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}
