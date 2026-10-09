using FluentValidation;
using IrisWeb.Application.Communication.Matrix.Requests;

namespace IrisWeb.Application.Validators.Matrix;

public sealed class OpenDeviceVacancyRequestValidator : AbstractValidator<OpenDeviceVacancyRequest>
{
    private const int MaxSeconds = 86_400;

    public OpenDeviceVacancyRequestValidator()
    {
        RuleFor(x => x.StartedSecondsAgo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O intervalo não pode ser negativo.")
            .LessThanOrEqualTo(MaxSeconds)
            .WithMessage($"O intervalo não pode passar de {MaxSeconds} segundos.");
    }
}
