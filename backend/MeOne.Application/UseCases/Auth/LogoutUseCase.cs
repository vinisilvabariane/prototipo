using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

public sealed class LogoutUseCase(IRefreshTokenRepository tokens, ICurrentActor currentActor) : ILogoutUseCase
{
    public async Task<Result> ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        if (currentActor.UserId is not { } actorId)
            return Result.Fail(new CredentialRejectedError(AuthErrors.SessionWithoutUser));

        if (request.UserId != actorId && !currentActor.IsInRole(AccessRoles.Master))
            return Result.Fail(new ForbiddenError(AuthErrors.LogoutOtherUserForbidden));

        await tokens.RevokeAllForUserAsync(request.UserId, cancellationToken);

        return Result.Ok();
    }
}
