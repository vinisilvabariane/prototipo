using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class RunsQueryValidator : AbstractValidator<RunsQuery>
{
    public RunsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0).WithMessage(PagingErrors.PageNegative);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagingErrors.MaxPageSize)
            .WithMessage(PagingErrors.PageSizeOutOfRange);

        RuleFor(x => x.Status!.Value).IsInEnum().When(x => x.Status is not null);
    }
}
