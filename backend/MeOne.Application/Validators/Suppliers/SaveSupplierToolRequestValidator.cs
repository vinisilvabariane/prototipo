using FluentValidation;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Suppliers;

public sealed class SaveSupplierToolRequestValidator : AbstractValidator<SaveSupplierToolRequest>
{
    public SaveSupplierToolRequestValidator()
    {
        RuleFor(x => x.Name).Required(SupplierErrors.ToolNameRequired, "O ferramental", SupplierErrors.MaxToolNameLength);
        RuleFor(x => x.AssetTag).Optional("O ativo fixo", SupplierErrors.MaxAssetTagLength);
    }
}
