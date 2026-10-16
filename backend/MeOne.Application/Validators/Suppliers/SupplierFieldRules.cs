using FluentValidation;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.Validators.Suppliers;

internal static class SupplierFieldRules
{
    public static IRuleBuilderOptions<T, string> Required<T>(
        this IRuleBuilder<T, string> rule,
        string requiredMessage,
        string field,
        int max) =>
        rule.NotEmpty().WithMessage(requiredMessage)
            .Must(value => value.Trim().Length <= max)
            .WithMessage(SupplierErrors.FieldTooLong(field, max));

    public static IRuleBuilderOptions<T, string?> Optional<T>(this IRuleBuilder<T, string?> rule, string field, int max) =>
        rule.Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= max)
            .WithMessage(SupplierErrors.FieldTooLong(field, max));

    public static IRuleBuilderOptions<T, decimal?> ValidScore<T>(this IRuleBuilder<T, decimal?> rule) =>
        rule.Must(QualificationRules.IsValidScore).WithMessage(SupplierErrors.ScoreInvalid);

    public static IRuleBuilderOptions<T, int?> ValidRevision<T>(this IRuleBuilder<T, int?> rule) =>
        rule.Must(value => value is null or >= 0 and <= SupplierErrors.MaxRevision).WithMessage(SupplierErrors.RevisionInvalid);
}
