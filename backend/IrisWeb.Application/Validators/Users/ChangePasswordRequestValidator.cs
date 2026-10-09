using FluentValidation;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Users;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(UserErrors.UserIdRequired);
        RuleFor(x => x.NewPassword).ValidNewPassword();
    }
}
