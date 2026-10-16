using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class CopyLayoutRequestValidator : AbstractValidator<CopyLayoutRequest>
{
    public CopyLayoutRequestValidator()
    {
        RuleFor(x => x.Targets).NotNull().WithMessage(ProductionErrors.CopyTargetsRequired);

        RuleForEach(x => x.Targets).ChildRules(target =>
        {
            target.RuleFor(item => item.LineId).GreaterThan(0UL).WithMessage(ProductionErrors.LineNotFound);
            target.RuleFor(item => item.ProductId).GreaterThan(0UL).WithMessage(ProductionErrors.ProductNotFound);
            target.RuleFor(item => item.ShiftId).GreaterThan(0UL).WithMessage(ProductionErrors.ShiftNotFound);
        }).When(x => x.Targets is not null);
    }
}
