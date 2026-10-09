using FluentValidation;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.Validators.Stations;

public sealed class LineTimelineQueryValidator : AbstractValidator<LineTimelineQuery>
{
    public LineTimelineQueryValidator()
    {
        RuleFor(x => x.Line)
            .NotEmpty().WithMessage(StationErrors.LineRequired)
            .MaximumLength(StationNamingPolicy.MaxFieldLength)
            .WithMessage(StationErrors.FieldTooLong("Linha", StationNamingPolicy.MaxFieldLength));

        RuleFor(x => x.Date).NotNull().WithMessage("Informe o dia.");

        RuleFor(x => x)
            .Must(x => x.From.HasValue == x.To.HasValue)
            .WithMessage("Informe o início e o fim da janela, ou nenhum dos dois.");
    }
}
