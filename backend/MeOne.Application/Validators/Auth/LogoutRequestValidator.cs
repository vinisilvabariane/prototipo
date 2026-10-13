using FluentValidation;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Auth;

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(UserErrors.UserIdRequired);
    }
}
