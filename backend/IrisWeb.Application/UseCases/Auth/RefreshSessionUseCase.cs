using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Communication.Auth.Responses;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Auth;

/// <summary>
/// Rotação: o refresh token usado é revogado e um novo é emitido. Falha responde 400 (não 401)
/// como no Access Control, para o interceptor do front não tentar renovar em laço.
/// </summary>
public sealed class RefreshSessionUseCase(
    IRefreshTokenRepository tokens,
    IUserRepository users,
    IAccessTokenIssuer accessTokens,
    IRefreshTokenGenerator refreshTokens,
    SessionPolicy policy) : IRefreshSessionUseCase
{
    public async Task<Result<AuthResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var stored = await tokens.GetByHashAsync(refreshTokens.Hash(request.RefreshToken.Trim()), cancellationToken);

        if (stored is null || !stored.IsUsable(DateTime.UtcNow))
            return Result.Fail(AuthErrors.InvalidRefreshToken);

        stored.Revoke();
        tokens.Update(stored);

        var user = await users.GetWithAccessAsync(stored.UserId, cancellationToken);

        if (user is null)
        {
            await tokens.SaveChangesAsync(cancellationToken);
            return Result.Fail(AuthErrors.InvalidRefreshToken);
        }

        return await AuthMapping.IssueSessionAsync(user, accessTokens, refreshTokens, tokens, policy, cancellationToken);
    }
}
