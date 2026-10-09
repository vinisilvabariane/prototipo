using FluentValidation;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Validators.Users;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Auth;

/// <summary>
/// Sem tamanho mínimo de senha no login: o Access Control cadastrava senhas de 6 caracteres e
/// barrava no login quem tinha menos de 8. O mínimo vale só para senha nova.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(AuthErrors.PasswordRequired)
            .MaximumLength(AuthErrors.MaxLoginPasswordLength)
            .WithMessage(AuthErrors.FieldTooLong("A senha", AuthErrors.MaxLoginPasswordLength));
    }
}
