using FluentValidation;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Helpdesk;

public sealed class RejectHelpdeskTicketRequestValidator : AbstractValidator<RejectHelpdeskTicketRequest>
{
    public RejectHelpdeskTicketRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(HelpdeskErrors.ReasonRequired)
            .Must(reason => reason is null || reason.Trim().Length <= HelpdeskErrors.MaxReasonLength)
            .WithMessage(HelpdeskErrors.ReasonTooLong);
    }
}
