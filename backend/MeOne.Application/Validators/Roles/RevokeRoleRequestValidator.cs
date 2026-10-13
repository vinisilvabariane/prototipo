using FluentValidation;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Roles;

public sealed class RevokeRoleRequestValidator : AbstractValidator<RevokeRoleRequest>
{
    public RevokeRoleRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(UserErrors.UserIdRequired);
        RuleFor(x => x.RoleId).GreaterThan(0UL).WithMessage(RoleErrors.RoleIdRequired);
    }
}
