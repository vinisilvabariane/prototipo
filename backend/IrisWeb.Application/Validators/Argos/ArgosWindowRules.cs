using FluentValidation;
using IrisWeb.Application.UseCases.Argos;

namespace IrisWeb.Application.Validators.Argos;

internal static class ArgosWindowRules
{
    public static void Apply<T>(AbstractValidator<T> validator, Func<T, DateTime?> from, Func<T, DateTime?> to)
    {
        validator.RuleFor(x => from(x))
            .NotNull()
            .WithMessage("Informe o início do período (from).");

        validator.RuleFor(x => to(x))
            .NotNull()
            .WithMessage("Informe o fim do período (to).");

        validator.RuleFor(x => x)
            .Must(x => to(x)!.Value > from(x)!.Value)
            .When(x => from(x).HasValue && to(x).HasValue)
            .WithMessage("A data final precisa ser depois da inicial.");

        validator.RuleFor(x => x)
            .Must(x => (to(x)!.Value - from(x)!.Value).TotalDays <= ListArgosCyclesUseCase.MaxRangeDays)
            .When(x => from(x).HasValue && to(x).HasValue)
            .WithMessage($"O período não pode passar de {ListArgosCyclesUseCase.MaxRangeDays} dias.");
    }
}
