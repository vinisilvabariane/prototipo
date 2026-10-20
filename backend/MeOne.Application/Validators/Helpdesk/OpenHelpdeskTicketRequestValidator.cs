using FluentValidation;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Helpdesk;

public sealed class OpenHelpdeskTicketRequestValidator : AbstractValidator<OpenHelpdeskTicketRequest>
{
    public OpenHelpdeskTicketRequestValidator()
    {
        RuleFor(x => x.SectorId).GreaterThan(0UL).WithMessage(HelpdeskErrors.SectorNotFound);
    }
}
