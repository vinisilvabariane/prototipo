using FluentValidation;
using IrisWeb.Application.Communication.Roles.Requests;

namespace IrisWeb.Application.Validators.Roles;

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).ValidRoleName();
        RuleFor(x => x.Description).ValidRoleDescription();
    }
}
