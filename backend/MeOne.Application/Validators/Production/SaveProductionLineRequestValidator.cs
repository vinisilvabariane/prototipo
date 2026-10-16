using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class SaveProductionLineRequestValidator : AbstractValidator<SaveProductionLineRequest>
{
    public SaveProductionLineRequestValidator()
    {
        RuleFor(x => x.Name).RequiredText(ProductionErrors.LineNameRequired, "O nome da linha", ProductionErrors.MaxLineNameLength);
    }
}
