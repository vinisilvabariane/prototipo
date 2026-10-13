using FluentValidation;
using MeOne.Application.Communication.Roles.Requests;

namespace MeOne.Application.Validators.Roles;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).ValidRoleName();
        RuleFor(x => x.Description).ValidRoleDescription();
    }
}
