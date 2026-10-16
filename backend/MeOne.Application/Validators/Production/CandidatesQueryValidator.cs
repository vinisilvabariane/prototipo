using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class CandidatesQueryValidator : AbstractValidator<CandidatesQuery>
{
    public CandidatesQueryValidator()
    {
        RuleFor(x => x.Search)
            .Must(search => search!.Trim().Length <= PagingErrors.MaxSearchLength)
            .When(x => x.Search is not null)
            .WithMessage(PagingErrors.SearchTooLong);
    }
}
