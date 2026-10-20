using FluentValidation;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.Validators.Oee;

public sealed class TestOeeFormulaRequestValidator : AbstractValidator<TestOeeFormulaRequest>
{
    public TestOeeFormulaRequestValidator()
    {
        RuleFor(x => x.Slot)
            .Must(slot => OeeFormulas.Slots.Any(candidate => candidate.Key == slot))
            .WithMessage(OeeErrors.ScopeInvalid);

        RuleFor(x => x.Expression)
            .Must(expression => (expression ?? string.Empty).Length <= OeeErrors.MaxFormulaLength)
            .WithMessage(OeeErrors.FormulaTooLong);
    }
}
