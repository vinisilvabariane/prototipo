using FluentValidation;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Workflows;

public sealed class WorkflowsQueryValidator : AbstractValidator<WorkflowsQuery>
{
    public WorkflowsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0).WithMessage(PagingErrors.PageNegative);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagingErrors.MaxPageSize)
            .WithMessage(PagingErrors.PageSizeOutOfRange);

        RuleFor(x => x.Filter).IsInEnum().WithMessage(WorkflowErrors.FilterInvalid);

        RuleFor(x => x.Search)
            .Must(search => search!.Trim().Length <= PagingErrors.MaxSearchLength)
            .When(x => x.Search is not null)
            .WithMessage(PagingErrors.SearchTooLong);
    }
}
