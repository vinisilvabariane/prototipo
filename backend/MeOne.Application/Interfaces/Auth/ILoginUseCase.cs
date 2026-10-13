using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Communication.Auth.Responses;

namespace MeOne.Application.Interfaces.Auth;

public interface ILoginUseCase
{
    Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken);
}
