using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Suppliers;

public sealed class SaveSupplierMaterialRequestValidator : AbstractValidator<SaveSupplierMaterialRequest>
{
    public SaveSupplierMaterialRequestValidator()
    {
        RuleFor(x => x.Sku).Required(SupplierErrors.SkuRequired, "O SKU", SupplierErrors.MaxSkuLength);
        RuleFor(x => x.BusinessUnit).Optional("A BU", SupplierErrors.MaxBusinessUnitLength);
        RuleFor(x => x.MaterialType).Optional("O tipo de material", SupplierErrors.MaxMaterialTypeLength);

        RuleFor(x => x.Criticality)
            .Must(criticality => Enum.IsDefined(criticality!.Value))
            .When(x => x.Criticality is not null)
            .WithMessage(SupplierErrors.CriticalityInvalid);
    }
}
