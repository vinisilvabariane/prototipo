using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Auth;

public sealed class ValidateCredentialsUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher) : IValidateCredentialsUseCase
{
    public async Task<Result> ExecuteAsync(ValidateCredentialsRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByRegistrationAsync(request.Registration, cancellationToken);

        return passwordHasher.Verify(request.Password, user?.PasswordHash) && user is not null
            ? Result.Ok()
            : Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));
    }
}
