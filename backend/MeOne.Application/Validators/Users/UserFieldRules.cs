using FluentValidation;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Users;

internal static class UserFieldRules
{
    public static IRuleBuilderOptions<T, uint> ValidRegistration<T>(this IRuleBuilder<T, uint> rule) =>
        rule.InclusiveBetween(UserErrors.MinRegistration, UserErrors.MaxRegistration)
            .WithMessage(UserErrors.RegistrationOutOfRange);

    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(UserErrors.NameRequired)
            .Must(name => name.Trim().Length is >= UserErrors.MinNameLength and <= UserErrors.MaxNameLength)
            .WithMessage(UserErrors.NameLength);

    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(email => email!.Trim().Length <= UserErrors.MaxEmailLength)
            .WithMessage(UserErrors.FieldTooLong("O e-mail", UserErrors.MaxEmailLength))
            .Must(email => IsEmail(email!))
            .WithMessage(UserErrors.EmailInvalid);

    public static IRuleBuilderOptions<T, string> ValidNewPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(UserErrors.PasswordRequired)
            .Length(UserErrors.MinPasswordLength, UserErrors.MaxPasswordLength)
            .WithMessage(UserErrors.PasswordLength);

    public static IRuleBuilderOptions<T, string> ValidDepartment<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(UserErrors.DepartmentRequired)
            .Must(name => name.Trim().Length <= DepartmentErrors.MaxNameLength)
            .WithMessage(DepartmentErrors.FieldTooLong("O departamento", DepartmentErrors.MaxNameLength));

    public static bool IsCorporate(string? email) =>
        email is not null && email.Trim().EndsWith(UserErrors.CorporateEmailDomain, StringComparison.OrdinalIgnoreCase);

    public static bool IsEmail(string email)
    {
        var trimmed = email.Trim();
        var at = trimmed.IndexOf('@');

        return at > 0 && at == trimmed.LastIndexOf('@') && at < trimmed.Length - 1 && !trimmed.Any(char.IsWhiteSpace);
    }
}
