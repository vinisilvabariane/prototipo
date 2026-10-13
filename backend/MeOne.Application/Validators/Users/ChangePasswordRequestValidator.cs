using FluentValidation;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Users;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(UserErrors.UserIdRequired);
        RuleFor(x => x.NewPassword).ValidNewPassword();
    }
}
