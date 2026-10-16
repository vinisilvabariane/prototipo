using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Suppliers;

public sealed class SaveSelfAssessmentRequestValidator : AbstractValidator<SaveSelfAssessmentRequest>
{
    public SaveSelfAssessmentRequestValidator()
    {
        RuleFor(x => x.ResponsibleId).GreaterThan(0UL).WithMessage(SupplierErrors.ResponsibleRequired);
        RuleFor(x => x.AssessedOn).NotEqual(default(DateOnly)).WithMessage(SupplierErrors.DateRequired);
        RuleFor(x => x.Status).IsInEnum().WithMessage(SupplierErrors.StatusInvalid);
        RuleFor(x => x.Score).ValidScore();
        RuleFor(x => x.Revision).ValidRevision();
        RuleFor(x => x.DocumentCode).Optional("O registro do manual", SupplierErrors.MaxDocumentCodeLength);
        RuleFor(x => x.EditableLink).Optional("O link", SupplierErrors.MaxLinkLength);
        RuleFor(x => x.EditableLink)
            .Must(link => Uri.TryCreate(link, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            .When(x => !string.IsNullOrWhiteSpace(x.EditableLink))
            .WithMessage(SupplierErrors.LinkInvalid);
    }
}
