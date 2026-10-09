using FluentResults;
using IrisWeb.Application.Communication.Auth.Responses;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Auth;

internal static class AuthMapping
{
    /// <summary>
    /// Emite o par JWT + refresh token. Persiste só o hash do refresh e devolve o valor cru ao
    /// cliente — o Access Control antigo devolvia o próprio hash, o que tornava o hash inútil.
    /// Salva também qualquer alteração pendente no repositório de tokens (ex.: o token revogado
    /// na rotação do refresh).
    /// </summary>
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
