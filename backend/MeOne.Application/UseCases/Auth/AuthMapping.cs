using FluentResults;
using MeOne.Application.Communication.Auth.Responses;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

internal static class AuthMapping
{
    public static async Task<Result<AuthResponse>> IssueSessionAsync(
        User user,
        IAccessTokenIssuer accessTokens,
        IRefreshTokenGenerator refreshTokens,
        IRefreshTokenRepository tokens,
        SessionPolicy policy,
        CancellationToken cancellationToken)
    {
        var rawRefreshToken = refreshTokens.Generate();
        var expiresAt = policy.RefreshExpiresAt(DateTime.UtcNow);

        await tokens.AddAsync(
            RefreshToken.Issue(user.Id, refreshTokens.Hash(rawRefreshToken), expiresAt),
            cancellationToken);

        if (await tokens.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(AuthErrors.NothingPersisted);

        return Result.Ok(new AuthResponse(accessTokens.Issue(user), rawRefreshToken, expiresAt));
    }
}
