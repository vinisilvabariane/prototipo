using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Suppliers;

public sealed class AddSupplierDocumentRequestValidator : AbstractValidator<AddSupplierDocumentRequest>
{
    public AddSupplierDocumentRequestValidator()
    {
        RuleFor(x => x.Kind).IsInEnum().WithMessage(SupplierErrors.DocumentKindInvalid);
        RuleFor(x => x.Version).Optional("A versão", SupplierErrors.MaxVersionLength);
    }
}
