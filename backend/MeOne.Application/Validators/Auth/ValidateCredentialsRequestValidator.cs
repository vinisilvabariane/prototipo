using FluentValidation;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Validators.Users;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Auth;

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
