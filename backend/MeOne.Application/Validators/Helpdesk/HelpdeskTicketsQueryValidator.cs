using FluentValidation;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Helpdesk;

public sealed class HelpdeskTicketsQueryValidator : AbstractValidator<HelpdeskTicketsQuery>
{
    public HelpdeskTicketsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0).WithMessage(PagingErrors.PageNegative);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, HelpdeskErrors.MaxPageSize)
            .WithMessage(PagingErrors.PageSizeOutOfRange);

        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.Status!.Value).IsInEnum().When(x => x.Status is not null);

        RuleFor(x => x.To!.Value)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .WithMessage(HelpdeskErrors.PeriodInvalid)
            .When(x => x.From is not null && x.To is not null);
    }
}
