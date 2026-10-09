using FluentValidation;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Validators.Users;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Auth;

public sealed class ValidateCredentialsRequestValidator : AbstractValidator<ValidateCredentialsRequest>
{
    public ValidateCredentialsRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(AuthErrors.PasswordRequired)
            .MaximumLength(AuthErrors.MaxLoginPasswordLength)
            .WithMessage(AuthErrors.FieldTooLong("A senha", AuthErrors.MaxLoginPasswordLength));
    }
}
