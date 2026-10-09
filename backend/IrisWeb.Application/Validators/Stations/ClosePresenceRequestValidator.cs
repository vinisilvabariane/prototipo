using FluentValidation;
using IrisWeb.Application.Communication.Stations.Requests;

namespace IrisWeb.Application.Validators.Stations;

public sealed class ClosePresenceRequestValidator : AbstractValidator<ClosePresenceRequest>
{

    private const int MaxGraceSeconds = 86_400;

    public ClosePresenceRequestValidator()
    {
        RuleFor(x => x.GraceSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("A tolerância não pode ser negativa.")
            .LessThanOrEqualTo(MaxGraceSeconds)
            .WithMessage($"A tolerância não pode passar de {MaxGraceSeconds} segundos.");
    }
}
