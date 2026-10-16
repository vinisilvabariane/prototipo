using FluentValidation;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Roles;

public sealed class SetRolePermissionsRequestValidator : AbstractValidator<SetRolePermissionsRequest>
{
    public const int MaxPermissionCodeLength = 50;

    public SetRolePermissionsRequestValidator()
    {
        RuleFor(x => x.Permissions).NotNull();

        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .MaximumLength(MaxPermissionCodeLength)
            .WithMessage((_, code) => RoleErrors.UnknownPermission(code ?? string.Empty));
    }
}
