using FluentResults;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Users;

public sealed class IssueTemporaryPasswordUseCase(
    IUserRepository users,
    IRefreshTokenRepository tokens,
    IPasswordHasher passwordHasher,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : IIssueTemporaryPasswordUseCase
{
    public async Task<Result<TemporaryPasswordResponse>> ExecuteAsync(ulong userId, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var user = await users.GetWithAccessAsync(userId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var dominance = AccessGuard.EnsureDominates(actor.Value, user);
        if (dominance.IsFailed)
            return dominance;

        var temporaryPassword = TemporaryPasswordPolicy.Generate();

        user.ChangePassword(passwordHasher.Hash(temporaryPassword), actor.Value.UserId);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        accessProfiles.Invalidate(user.Id);

        return Result.Ok(new TemporaryPasswordResponse(temporaryPassword));
    }
}
