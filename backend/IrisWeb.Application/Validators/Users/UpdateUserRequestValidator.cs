using FluentValidation;
using IrisWeb.Application.Communication.Users.Requests;

namespace IrisWeb.Application.Validators.Users;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();
        RuleFor(x => x.Name).ValidName();
        RuleFor(x => x.Department).ValidDepartment();

        RuleFor(x => x.Email)
            .ValidEmail()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
