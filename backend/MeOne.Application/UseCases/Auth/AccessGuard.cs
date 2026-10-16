using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

internal static class AccessGuard
{
    public static async Task<Result<AccessProfile>> ActorAsync(ICurrentActor currentActor, CancellationToken cancellationToken)
    {
        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return profile is { Active: true }
            ? Result.Ok(profile)
            : Result.Fail(new CredentialRejectedError(AuthErrors.SessionWithoutUser));
    }

    public static Result EnsureDominates(AccessProfile actor, User target) =>
        actor.UserId == target.Id || actor.Dominates(AccessProfile.From(target))
            ? Result.Ok()
            : Result.Fail(new ForbiddenError(UserErrors.TargetHasMorePrivileges));

    public static async Task<Result> EnsureNotLastAdministratorAsync(
        IUserRepository users,
        User target,
        CancellationToken cancellationToken)
    {
        if (!target.IsAdministrator)
            return Result.Ok();

        return await users.CountActiveAdministratorsAsync(cancellationToken) > 1
            ? Result.Ok()
            : Result.Fail(UserErrors.LastAdministrator);
    }

    public static bool HashesMatch(string expected, string actual) =>
        System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(expected),
            System.Text.Encoding.UTF8.GetBytes(actual));
}
