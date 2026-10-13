using FluentResults;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Users;

public sealed class ResetPasswordUseCase(
    IUserRepository users,
    IRefreshTokenRepository tokens,
    IPasswordHasher passwordHasher,
    IPasswordResetNotifier notifier,
    ICurrentActor currentActor) : IResetPasswordUseCase
{
    public async Task<Result<ResetPasswordResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken)
    {
        var user = await users.GetByRegistrationAsync(registration, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var byMaster = currentActor.IsInRole(AccessRoles.Master);

        if (!byMaster && user.Email is null)
            return Result.Fail(UserErrors.NoEmailForReset);

        var actorId = UsersMapping.ActorId(currentActor);
        var previousHash = user.PasswordHash;
        var temporaryPassword = TemporaryPasswordPolicy.Generate();

        user.ChangePassword(passwordHasher.Hash(temporaryPassword), actorId);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        var emailSent = user.Email is not null &&
                        await notifier.NotifyAsync(user, temporaryPassword, cancellationToken);

        if (!byMaster && !emailSent)
        {
            user.ChangePassword(previousHash, actorId);
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);

            return Result.Fail(UserErrors.ResetEmailFailed);
        }

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return Result.Ok(new ResetPasswordResponse(byMaster ? temporaryPassword : null, emailSent));
    }
}
