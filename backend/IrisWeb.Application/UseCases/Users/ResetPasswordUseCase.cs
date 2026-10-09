using FluentResults;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Users;

/// <summary>
/// "Esqueci a senha". Anônimo: a senha provisória vai só por e-mail — se o usuário não tem e-mail
/// ou o envio falha, a senha antiga é mantida. Master: recebe a senha provisória na resposta
/// (para repassar a quem não tem e-mail). O Access Control antigo devolvia a senha nova para
/// qualquer um que soubesse a matrícula.
/// </summary>
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
