using FluentValidation;
using MeOne.Application.Communication.Roles.Requests;

namespace MeOne.Application.Validators.Roles;

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Name).ValidRoleName();
        RuleFor(x => x.Description).ValidRoleDescription();
    }
}
