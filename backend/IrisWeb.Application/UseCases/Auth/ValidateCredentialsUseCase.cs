using FluentResults;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Auth;

/// <summary>Só confere matrícula e senha, sem abrir sessão (ex.: confirmar identidade numa ação).</summary>
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
