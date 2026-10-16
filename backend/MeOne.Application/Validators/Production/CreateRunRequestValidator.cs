using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class CreateRunRequestValidator : AbstractValidator<CreateRunRequest>
{
    public CreateRunRequestValidator()
    {
        RuleFor(x => x.LayoutId).GreaterThan(0UL).WithMessage(ProductionErrors.LayoutNotFound);
    }
}
