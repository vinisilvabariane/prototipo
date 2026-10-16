using System.Globalization;
using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Auth;

public sealed class ValidateCredentialsUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor) : IValidateCredentialsUseCase
{
    public async Task<Result> ExecuteAsync(ValidateCredentialsRequest request, CancellationToken cancellationToken)
    {
        if (currentActor.Registration != request.Registration.ToString(CultureInfo.InvariantCulture))
            return Result.Fail(new ForbiddenError(AuthErrors.ValidateOtherUserForbidden));

        var now = DateTime.UtcNow;
        var user = await users.GetByRegistrationAsync(request.Registration, cancellationToken);
        var passwordMatches = passwordHasher.Verify(request.Password, user?.PasswordHash);

        if (user is null || user.IsLockedOut(now))
            return Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));

        if (!passwordMatches)
        {
            user.RegisterFailedLogin(now);
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);

            return Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));
        }

        if (user.RegisterSuccessfulLogin())
        {
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }
}
