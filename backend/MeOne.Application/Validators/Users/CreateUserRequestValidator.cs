using FluentValidation;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Users;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();
        RuleFor(x => x.Name).ValidName();
        RuleFor(x => x.Password).ValidNewPassword();
        RuleFor(x => x.Department).ValidDepartment();

        RuleFor(x => x.Email)
            .ValidEmail()
            .Must(UserFieldRules.IsCorporate).WithMessage(UserErrors.EmailNotCorporate)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
