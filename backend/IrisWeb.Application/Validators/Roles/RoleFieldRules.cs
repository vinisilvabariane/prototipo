using FluentValidation;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.Roles;

internal static class RoleFieldRules
{
    public static IRuleBuilderOptions<T, string> ValidRoleName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(RoleErrors.NameRequired)
            .Must(name => name.Trim().Length <= RoleErrors.MaxNameLength)
            .WithMessage(RoleErrors.FieldTooLong("O nome da role", RoleErrors.MaxNameLength));

    public static IRuleBuilderOptions<T, string> ValidRoleDescription<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(RoleErrors.DescriptionRequired)
            .Must(description => description.Trim().Length <= RoleErrors.MaxDescriptionLength)
            .WithMessage(RoleErrors.FieldTooLong("A descrição", RoleErrors.MaxDescriptionLength));
}
