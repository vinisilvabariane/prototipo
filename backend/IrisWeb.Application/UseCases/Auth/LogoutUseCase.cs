using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Auth;

/// <summary>
/// Qualquer usuário encerra a própria sessão; só master encerra a de outro. Sem sessão ativa
/// para revogar o logout continua sendo sucesso.
/// </summary>
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
