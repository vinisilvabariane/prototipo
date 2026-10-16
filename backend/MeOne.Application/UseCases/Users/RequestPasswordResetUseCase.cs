using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Users;

public sealed class RequestPasswordResetUseCase(
    IUserRepository users,
    IPasswordResetTokenRepository resetTokens,
    IRefreshTokenGenerator secrets,
    IPasswordResetNotifier notifier) : IRequestPasswordResetUseCase
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

    public async Task<Result> ExecuteAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await users.GetByRegistrationAsync(request.Registration, cancellationToken);

        if (user?.Email is null)
            return Result.Ok();

        var latest = await resetTokens.GetLatestForUserAsync(user.Id, cancellationToken);
        if (latest is not null && latest.ExpiresAt - PasswordResetPolicy.CodeLifetime > now - MinimumInterval)
            return Result.Ok();

        await resetTokens.InvalidateAllForUserAsync(user.Id, now, cancellationToken);

        var code = PasswordResetPolicy.GenerateCode();
        var expiresAt = now.Add(PasswordResetPolicy.CodeLifetime);

        await resetTokens.AddAsync(PasswordResetToken.Issue(user.Id, secrets.Hash(code), expiresAt), cancellationToken);
        await resetTokens.SaveChangesAsync(cancellationToken);

        _ = notifier.SendResetCodeAsync(user, code, expiresAt, CancellationToken.None);

        return Result.Ok();
    }
}
