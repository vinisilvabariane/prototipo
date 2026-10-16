using FluentValidation;
using MeOne.Application.Communication.Scopes.Requests;

namespace MeOne.Application.Validators.Scopes;

public sealed class UpdateScopeRequestValidator : AbstractValidator<UpdateScopeRequest>
{
    public UpdateScopeRequestValidator()
    {
        RuleFor(x => x.Code).ValidScopeCode();
        RuleFor(x => x.Name).ValidScopeName();
    }
}
