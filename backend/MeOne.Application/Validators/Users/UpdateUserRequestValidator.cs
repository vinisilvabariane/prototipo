using FluentValidation;
using MeOne.Application.Communication.Users.Requests;

namespace MeOne.Application.Validators.Users;

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
