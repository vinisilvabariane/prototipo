using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class CreateLayoutRequestValidator : AbstractValidator<CreateLayoutRequest>
{
    public CreateLayoutRequestValidator()
    {
        RuleFor(x => x.LineId).GreaterThan(0UL).WithMessage(ProductionErrors.LineNotFound);
        RuleFor(x => x.ProductId).GreaterThan(0UL).WithMessage(ProductionErrors.ProductNotFound);
        RuleFor(x => x.ShiftId).GreaterThan(0UL).WithMessage(ProductionErrors.ShiftNotFound);
    }
}
