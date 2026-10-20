using FluentValidation;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Helpdesk;

public sealed class SaveHelpdeskFieldTemplateRequestValidator : AbstractValidator<SaveHelpdeskFieldTemplateRequest>
{
    public SaveHelpdeskFieldTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(HelpdeskErrors.NameRequired)
            .Must(name => name is null || name.Trim().Length <= HelpdeskErrors.MaxNameLength)
            .WithMessage(HelpdeskErrors.FieldTooLong("O nome", HelpdeskErrors.MaxNameLength));

        RuleFor(x => x.Label).NotEmpty().WithMessage(HelpdeskErrors.FieldLabelRequired);
        RuleFor(x => x.Type).IsInEnum();
    }
}
