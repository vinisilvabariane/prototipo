using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Users;

public sealed class ConfirmPasswordResetUseCase(
    IUserRepository users,
    IPasswordResetTokenRepository resetTokens,
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenGenerator secrets,
    IPasswordHasher passwordHasher,
    IAccessProfileCache accessProfiles) : IConfirmPasswordResetUseCase
{
    public async Task<Result> ExecuteAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await users.GetByRegistrationAsync(request.Registration, cancellationToken);
        var token = user is null ? null : await resetTokens.GetLatestForUserAsync(user.Id, cancellationToken);
        var codeHash = secrets.Hash(PasswordResetPolicy.NormalizeCode(request.Code));

        if (user is null || token is null || !token.IsUsable(now))
            return Result.Fail(AuthErrors.InvalidResetCode);

        if (!AccessGuard.HashesMatch(token.CodeHash, codeHash))
        {
            token.RegisterFailedAttempt();
            resetTokens.Update(token);
            await resetTokens.SaveChangesAsync(cancellationToken);

            return Result.Fail(AuthErrors.InvalidResetCode);
        }

        token.MarkUsed(now);
        resetTokens.Update(token);
        await resetTokens.SaveChangesAsync(cancellationToken);

        user.ChangePassword(passwordHasher.Hash(request.NewPassword), user.Id);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        await refreshTokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        accessProfiles.Invalidate(user.Id);

        return Result.Ok();
    }
}
