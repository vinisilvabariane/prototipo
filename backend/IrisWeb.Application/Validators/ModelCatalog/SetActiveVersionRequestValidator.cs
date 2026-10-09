using FluentValidation;
using IrisWeb.Application.Communication.ModelCatalog.Requests;

namespace IrisWeb.Application.Validators.ModelCatalog;

public sealed class SetActiveVersionRequestValidator : AbstractValidator<SetActiveVersionRequest>
{
    public SetActiveVersionRequestValidator()
    {
        RuleFor(x => x.VersionId).NotEmpty();
    }
}
