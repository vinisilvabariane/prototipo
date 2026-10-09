using FluentValidation;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.Validators.Stations;

public sealed class CloseUnknownRequestValidator : AbstractValidator<CloseUnknownRequest>
{
    private const int MaxSeconds = 86_400;

    public CloseUnknownRequestValidator()
    {
        RuleFor(x => x.EndedSecondsAgo)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O intervalo não pode ser negativo.")
            .LessThanOrEqualTo(MaxSeconds)
            .WithMessage($"O intervalo não pode passar de {MaxSeconds} segundos.");

        RuleFor(x => x.CandidateRegistration)
            .MaximumLength(StationNamingPolicy.MaxRegistrationLength)
            .WithMessage(StationErrors.FieldTooLong("Matrícula candidata", StationNamingPolicy.MaxRegistrationLength));

        RuleFor(x => x.CandidateDistance)
            .GreaterThanOrEqualTo(0)
            .When(x => x.CandidateDistance is not null)
            .WithMessage("A distância não pode ser negativa.");
    }
}
