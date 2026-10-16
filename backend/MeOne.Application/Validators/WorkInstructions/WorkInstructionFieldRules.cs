using FluentValidation;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.WorkInstructions;

internal static class WorkInstructionFieldRules
{
    public static IRuleBuilderOptions<T, string> Required<T>(
        this IRuleBuilder<T, string> rule,
        string requiredMessage,
        string field,
        int max) =>
        rule.NotEmpty().WithMessage(requiredMessage)
            .Must(value => value.Trim().Length <= max)
            .WithMessage(WorkInstructionErrors.FieldTooLong(field, max));
}
