using FluentValidation;
using IrisWeb.Application.Communication.Argos.Requests;

namespace IrisWeb.Application.Validators.Argos;

public sealed class ArgosSummaryQueryValidator : AbstractValidator<ArgosSummaryQuery>
{
    public ArgosSummaryQueryValidator()
    {
        ArgosWindowRules.Apply(this, query => query.From, query => query.To);
    }
}
