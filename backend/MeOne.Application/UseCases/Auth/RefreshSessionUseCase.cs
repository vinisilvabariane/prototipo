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
    IAccessProfileCache accessProfiles,
    SessionPolicy policy) : IRefreshSessionUseCase
{
    public async Task<Result<AuthResponse>> ExecuteAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var stored = await tokens.GetByHashAsync(refreshTokens.Hash(request.RefreshToken.Trim()), cancellationToken);

        if (stored is null)
            return Result.Fail(AuthErrors.InvalidRefreshToken);

        if (stored.Revoked)
        {
            await RevokeEverySessionAsync(stored.UserId, cancellationToken);
            return Result.Fail(AuthErrors.InvalidRefreshToken);
        }

        if (!stored.IsUsable(DateTime.UtcNow) || !await tokens.TryRevokeAsync(stored.Id, cancellationToken))
            return Result.Fail(AuthErrors.InvalidRefreshToken);

        var user = await users.GetWithAccessAsync(stored.UserId, cancellationToken);

        if (user is null)
            return Result.Fail(AuthErrors.InvalidRefreshToken);

        return await AuthMapping.IssueSessionAsync(user, accessTokens, refreshTokens, tokens, policy, cancellationToken);
    }

    private async Task RevokeEverySessionAsync(ulong userId, CancellationToken cancellationToken)
    {
        await tokens.RevokeAllForUserAsync(userId, cancellationToken);

        var user = await users.GetByIdAsync(userId, cancellationToken);

        if (user is not null)
        {
            user.RotateSecurityStamp();
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);
        }

        accessProfiles.Invalidate(userId);
    }
}
