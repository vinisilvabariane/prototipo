using FluentValidation;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Helpdesk;

public sealed class ResolveHelpdeskTicketRequestValidator : AbstractValidator<ResolveHelpdeskTicketRequest>
{
    public ResolveHelpdeskTicketRequestValidator()
    {
        RuleFor(x => x.Note)
            .Must(note => note is null || note.Trim().Length <= HelpdeskErrors.MaxNoteLength)
            .WithMessage(HelpdeskErrors.NoteTooLong);
    }
}
