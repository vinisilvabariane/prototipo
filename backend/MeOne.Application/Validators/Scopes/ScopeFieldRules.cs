using FluentValidation;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Scopes;

internal static class ScopeFieldRules
{
    public static IRuleBuilderOptions<T, string> ValidScopeCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(ScopeErrors.CodeRequired)
            .Must(code => code.Trim().Length <= ScopeErrors.MaxCodeLength)
            .WithMessage(ScopeErrors.FieldTooLong("O código", ScopeErrors.MaxCodeLength));

    public static IRuleBuilderOptions<T, string> ValidScopeName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(ScopeErrors.NameRequired)
            .Must(name => name.Trim().Length <= ScopeErrors.MaxNameLength)
            .WithMessage(ScopeErrors.FieldTooLong("O nome", ScopeErrors.MaxNameLength));
}
