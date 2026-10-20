using FluentValidation;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.UseCases.Oee;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.Validators.Oee;

public sealed class SaveOeeFormulaSetRequestValidator : AbstractValidator<SaveOeeFormulaSetRequest>
{
    public SaveOeeFormulaSetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(OeeErrors.SetNameRequired)
            .Must(name => name is null || name.Trim().Length <= OeeErrors.MaxSetNameLength)
            .WithMessage($"O nome pode ter no máximo {OeeErrors.MaxSetNameLength} caracteres.");

        RuleFor(x => x.Formulas)
            .NotNull().WithMessage(OeeErrors.FormulaRequired)
            .Custom((formulas, context) =>
            {
                if (formulas is null)
                    return;

                var message = OeeMapping.ToDomain(formulas).Check();

                if (message is not null)
                    context.AddFailure(message);
            });
    }
}
