using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Communication.Auth.Responses;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

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
