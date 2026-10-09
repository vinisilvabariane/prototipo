using FluentValidation;
using IrisWeb.Application.Communication.ModelCatalog.Requests;

namespace IrisWeb.Application.Validators.ModelCatalog;

public sealed class CreateModelVersionRequestValidator : AbstractValidator<CreateModelVersionRequest>
{
    public CreateModelVersionRequestValidator()
    {
        RuleFor(x => x.Version).NotEmpty().MaximumLength(64)
            .Matches(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z.-]+)?$")
            .WithMessage("Version deve seguir SemVer, por exemplo 1.2.0 ou 1.2.0-beta.1.")

            .Must(version => version is null || !version.Contains("..", StringComparison.Ordinal))
            .WithMessage("Version nao pode conter \"..\".")
            .Must(version => version is null || !version.EndsWith('.') && !version.EndsWith(' '))
            .WithMessage("Version nao pode terminar em ponto ou espaco.");
        RuleFor(x => x.Runtime).NotEmpty().MaximumLength(64);
    }
}
