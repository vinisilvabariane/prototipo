using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Communication.Auth.Responses;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Auth;

/// <summary>
/// Uma sessão por usuário: o login revoga todos os refresh tokens anteriores dele.
/// </summary>
public sealed class LoginUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokens,
    IRefreshTokenGenerator refreshTokens,
    IRefreshTokenRepository tokens,
    SessionPolicy policy) : ILoginUseCase
{
    public async Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessByRegistrationAsync(request.Registration, cancellationToken);

        if (!passwordHasher.Verify(request.Password, user?.PasswordHash) || user is null)
            return Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return await AuthMapping.IssueSessionAsync(user, accessTokens, refreshTokens, tokens, policy, cancellationToken);
    }
}
