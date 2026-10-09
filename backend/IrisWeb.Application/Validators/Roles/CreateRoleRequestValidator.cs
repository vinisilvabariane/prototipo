using FluentValidation;
using IrisWeb.Application.Communication.Roles.Requests;

namespace IrisWeb.Application.Validators.Roles;

public sealed class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name).ValidRoleName();
        RuleFor(x => x.Description).ValidRoleDescription();
    }
}
