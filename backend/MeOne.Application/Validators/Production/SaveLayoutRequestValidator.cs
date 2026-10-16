using FluentValidation;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Errors;

namespace MeOne.Application.Validators.Production;

public sealed class SaveLayoutRequestValidator : AbstractValidator<SaveLayoutRequest>
{
    public SaveLayoutRequestValidator()
    {
        RuleFor(x => x.Posts).NotNull().WithMessage("Envie a lista de postos.");
        RuleFor(x => x.Decorations).NotNull().WithMessage("Envie a lista de decorações.");

        RuleForEach(x => x.Posts).ChildRules(post =>
        {
            post.RuleFor(item => item.Kind).IsInEnum().WithMessage(ProductionErrors.PostKindInvalid);
        }).When(x => x.Posts is not null);

        RuleForEach(x => x.Decorations).ChildRules(decoration =>
        {
            decoration.RuleFor(item => item.Kind).IsInEnum().WithMessage(ProductionErrors.DecorationKindInvalid);
        }).When(x => x.Decorations is not null);
    }
}
