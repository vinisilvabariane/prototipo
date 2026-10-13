using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Communication.Auth.Responses;

namespace MeOne.Application.Interfaces.Auth;

public interface IRefreshSessionUseCase
{
    Task<Result<AuthResponse>> ExecuteAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}
