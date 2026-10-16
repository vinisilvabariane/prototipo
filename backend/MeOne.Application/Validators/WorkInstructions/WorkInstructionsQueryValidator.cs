using FluentValidation;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.WorkInstructions;

public sealed class WorkInstructionsQueryValidator : AbstractValidator<WorkInstructionsQuery>
{
    public WorkInstructionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0).WithMessage(PagingErrors.PageNegative);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagingErrors.MaxPageSize)
            .WithMessage(PagingErrors.PageSizeOutOfRange);

        RuleFor(x => x.Search)
            .Must(search => search!.Trim().Length <= PagingErrors.MaxSearchLength)
            .When(x => x.Search is not null)
            .WithMessage(PagingErrors.SearchTooLong);
    }
}
