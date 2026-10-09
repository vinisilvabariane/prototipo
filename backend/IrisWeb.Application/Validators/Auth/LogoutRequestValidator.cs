using FluentValidation;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Auth;

public sealed class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(UserErrors.UserIdRequired);
    }
}
