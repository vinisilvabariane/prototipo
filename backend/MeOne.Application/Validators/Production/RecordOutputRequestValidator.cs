using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class RecordOutputRequestValidator : AbstractValidator<RecordOutputRequest>
{
    public RecordOutputRequestValidator()
    {
        RuleFor(x => x.PostId).GreaterThan(0UL).WithMessage(ProductionErrors.PostNotInRun);
        RuleFor(x => x.Quantity).InclusiveBetween(0, ProductionRun.MaxQuantity).WithMessage(ProductionErrors.QuantityInvalid);
    }
}
