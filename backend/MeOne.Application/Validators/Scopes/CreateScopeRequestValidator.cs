using FluentValidation;
using MeOne.Application.Communication.Scopes.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Scopes;

public sealed class CreateScopeRequestValidator : AbstractValidator<CreateScopeRequest>
{
    public CreateScopeRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage(ScopeErrors.TypeInvalid);
        RuleFor(x => x.Code).ValidScopeCode();
        RuleFor(x => x.Name).ValidScopeName();
        RuleFor(x => x.ParentId).GreaterThan(0UL).When(x => x.ParentId is not null).WithMessage(ScopeErrors.NotFound);
    }
}
