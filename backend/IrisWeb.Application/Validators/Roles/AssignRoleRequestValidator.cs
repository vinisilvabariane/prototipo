using FluentValidation;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Roles;

public sealed class AssignRoleRequestValidator : AbstractValidator<AssignRoleRequest>
{
    public AssignRoleRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(UserErrors.UserIdRequired);
        RuleFor(x => x.RoleId).GreaterThan(0UL).WithMessage(RoleErrors.RoleIdRequired);
    }
}
