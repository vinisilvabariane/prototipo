using FluentValidation;
using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.UseCases.Argos;

namespace IrisWeb.Application.Validators.Argos;

public sealed class ArgosCyclesQueryValidator : AbstractValidator<ArgosCyclesQuery>
{
    public ArgosCyclesQueryValidator()
    {
        ArgosWindowRules.Apply(this, query => query.From, query => query.To);

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Page.HasValue)
            .WithMessage("A página começa em 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, ListArgosCyclesUseCase.MaxPageSize)
            .When(x => x.PageSize.HasValue)
            .WithMessage($"O tamanho da página vai de 1 a {ListArgosCyclesUseCase.MaxPageSize}.");
    }
}
