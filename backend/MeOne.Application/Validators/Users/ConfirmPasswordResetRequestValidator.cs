using FluentValidation;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.Validators.Users;

public sealed class ConfirmPasswordResetRequestValidator : AbstractValidator<ConfirmPasswordResetRequest>
{
    public ConfirmPasswordResetRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(AuthErrors.ResetCodeRequired)
            .Must(code => code.Trim().Length == PasswordResetPolicy.CodeLength)
            .WithMessage(AuthErrors.InvalidResetCode);

        RuleFor(x => x.NewPassword).ValidNewPassword();
    }
}
