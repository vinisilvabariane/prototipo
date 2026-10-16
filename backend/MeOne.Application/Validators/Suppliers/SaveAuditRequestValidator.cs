using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.Validators.Suppliers;

public sealed class SaveAuditRequestValidator : AbstractValidator<SaveAuditRequest>
{
    public SaveAuditRequestValidator()
    {
        RuleFor(x => x.ResponsibleId).GreaterThan(0UL).WithMessage(SupplierErrors.ResponsibleRequired);
        RuleFor(x => x.AuditedOn).NotEqual(default(DateOnly)).WithMessage(SupplierErrors.DateRequired);
        RuleFor(x => x.Status).IsInEnum().WithMessage(SupplierErrors.StatusInvalid);
        RuleFor(x => x.ValidityMonths).Must(QualificationRules.AuditValidityMonths.Contains).WithMessage(SupplierErrors.ValidityInvalid);
        RuleFor(x => x.Score).ValidScore();
        RuleFor(x => x.ChecklistRevision).ValidRevision();
        RuleFor(x => x.ChecklistCode).Optional("O registro do checklist", SupplierErrors.MaxDocumentCodeLength);
    }
}
