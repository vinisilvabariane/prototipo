using FluentValidation;
using IrisWeb.Application.Communication.ModelCatalog.Requests;

namespace IrisWeb.Application.Validators.ModelCatalog;

public sealed class CreateModelRequestValidator : AbstractValidator<CreateModelRequest>
{
    public CreateModelRequestValidator()
    {

        RuleFor(x => x.Key).NotEmpty().MaximumLength(100).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.TaskType).IsInEnum();
        RuleFor(x => x.Platform).IsInEnum();
    }
}
