using FluentValidation;
using MeOne.Application.Communication.Users.Requests;

namespace MeOne.Application.Validators.Users;

public sealed class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();
    }
}
