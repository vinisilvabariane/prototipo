using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.Validators.Suppliers;

public sealed class SaveSupplierRequestValidator : AbstractValidator<SaveSupplierRequest>
{
    public SaveSupplierRequestValidator()
    {
        RuleFor(x => x.SapCode).Required(SupplierErrors.SapCodeRequired, "O código SAP", SupplierErrors.MaxSapCodeLength);
        RuleFor(x => x.Name).Required(SupplierErrors.NameRequired, "O nome fantasia", SupplierErrors.MaxNameLength);
        RuleFor(x => x.Branch).Required(SupplierErrors.BranchRequired, "A filial do fornecedor", SupplierErrors.MaxBranchLength);
        RuleFor(x => x.ZipCode).Required(SupplierErrors.ZipCodeRequired, "O CEP", SupplierErrors.MaxZipCodeLength);
        RuleFor(x => x.District).Required(SupplierErrors.DistrictRequired, "O bairro", SupplierErrors.MaxDistrictLength);

        RuleFor(x => x.Origin).IsInEnum().WithMessage(SupplierErrors.OriginInvalid);
        RuleFor(x => x.MultiBranch).IsInEnum().WithMessage(SupplierErrors.MultiBranchInvalid);
        RuleFor(x => x.Criticality).IsInEnum().WithMessage(SupplierErrors.CriticalityInvalid);

        RuleFor(x => x.TaxId).Optional("O CNPJ", SupplierErrors.MaxTaxIdLength);
        RuleFor(x => x.TaxId)
            .Must(taxId => Cnpj.IsValid(taxId!))
            .When(x => x.Origin == SupplierOrigin.Domestic && !string.IsNullOrWhiteSpace(x.TaxId))
            .WithMessage(SupplierErrors.CnpjInvalid);

        RuleFor(x => x.CorporateName).Optional("A razão social", SupplierErrors.MaxCorporateNameLength);
        RuleFor(x => x.Street).Optional("O logradouro", SupplierErrors.MaxStreetLength);
        RuleFor(x => x.Number).Optional("O número", SupplierErrors.MaxNumberLength);
        RuleFor(x => x.ContactName).Optional("O responsável", SupplierErrors.MaxContactLength);
        RuleFor(x => x.Phone).Optional("O telefone", SupplierErrors.MaxPhoneLength);

        RuleFor(x => x.Email).Optional("O e-mail", SupplierErrors.MaxEmailLength);
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(SupplierErrors.EmailInvalid);
    }
}
