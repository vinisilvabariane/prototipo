using FluentValidation;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Oee;

public sealed class OeeRunsQueryValidator : AbstractValidator<OeeRunsQuery>
{
    public OeeRunsQueryValidator()
    {
        RuleFor(x => x.Status!.Value).IsInEnum().When(x => x.Status is not null);

        RuleFor(x => x.To!.Value)
            .GreaterThanOrEqualTo(x => x.From!.Value)
            .WithMessage(OeeErrors.PeriodInvalid)
            .When(x => x.From is not null && x.To is not null);
    }
}
