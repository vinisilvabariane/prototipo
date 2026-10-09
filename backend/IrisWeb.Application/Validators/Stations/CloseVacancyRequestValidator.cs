using FluentValidation;
using IrisWeb.Application.Communication.Stations.Requests;

namespace IrisWeb.Application.Validators.Stations;

public sealed class CloseVacancyRequestValidator : AbstractValidator<CloseVacancyRequest>
{

    private const int MaxSeconds = 86_400;

    public CloseVacancyRequestValidator()
    {
        RuleFor(x => x.EndedSecondsAgo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O intervalo não pode ser negativo.")
            .LessThanOrEqualTo(MaxSeconds)
            .WithMessage($"O intervalo não pode passar de {MaxSeconds} segundos.");
    }
}
