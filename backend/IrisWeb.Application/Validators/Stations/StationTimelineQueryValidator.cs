using FluentValidation;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.UseCases.Stations;

namespace IrisWeb.Application.Validators.Stations;

public sealed class StationTimelineQueryValidator : AbstractValidator<StationTimelineQuery>
{
    public StationTimelineQueryValidator()
    {
        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("A data final precisa ser depois da inicial.");

        RuleFor(x => x)
            .Must(x => (x.To!.Value - x.From!.Value).TotalDays <= ListStationTimelineUseCase.MaxRangeDays)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage($"O período não pode passar de {ListStationTimelineUseCase.MaxRangeDays} dias.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Page.HasValue)
            .WithMessage("A página começa em 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, ListStationTimelineUseCase.MaxPageSize)
            .When(x => x.PageSize.HasValue)
            .WithMessage($"O tamanho da página vai de 1 a {ListStationTimelineUseCase.MaxPageSize}.");

        RuleFor(x => x.Registration)
            .Matches("^[0-9]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.Registration))
            .WithMessage("A matrícula deve conter apenas dígitos.");
    }
}
