using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class AllocatePostRequestValidator : AbstractValidator<AllocatePostRequest>
{
    public AllocatePostRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0UL).WithMessage(ProductionErrors.UserNotFound);
    }
}
