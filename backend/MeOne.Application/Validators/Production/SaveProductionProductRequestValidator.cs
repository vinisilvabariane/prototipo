using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class SaveProductionProductRequestValidator : AbstractValidator<SaveProductionProductRequest>
{
    public SaveProductionProductRequestValidator()
    {
        RuleFor(x => x.Code).RequiredText(ProductionErrors.ProductCodeRequired, "O código do produto", ProductionErrors.MaxProductCodeLength);
        RuleFor(x => x.Name).RequiredText(ProductionErrors.ProductNameRequired, "O nome do produto", ProductionErrors.MaxProductNameLength);
    }
}
