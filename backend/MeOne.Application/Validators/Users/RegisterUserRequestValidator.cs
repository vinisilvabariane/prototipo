using FluentValidation;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Users;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Registration).ValidRegistration();
        RuleFor(x => x.Name).ValidName();
        RuleFor(x => x.Password).ValidNewPassword();
        RuleFor(x => x.Department).ValidDepartment();

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(UserErrors.EmailRequired)
            .Must(email => email.Trim().Length <= UserErrors.MaxEmailLength)
            .WithMessage(UserErrors.FieldTooLong("O e-mail", UserErrors.MaxEmailLength))
            .Must(UserFieldRules.IsEmail).WithMessage(UserErrors.EmailInvalid);
    }
}
