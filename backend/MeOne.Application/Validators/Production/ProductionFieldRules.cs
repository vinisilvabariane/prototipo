using FluentValidation;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

internal static class ProductionFieldRules
{
    public static IRuleBuilderOptions<T, string> RequiredText<T>(
        this IRuleBuilder<T, string> rule,
        string requiredMessage,
        string field,
        int max) =>
        rule.NotEmpty().WithMessage(requiredMessage)
            .Must(value => value.Trim().Length <= max)
            .WithMessage(ProductionErrors.FieldTooLong(field, max));
}
