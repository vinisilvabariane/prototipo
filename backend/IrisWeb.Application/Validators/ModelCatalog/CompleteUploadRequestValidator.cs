using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using IrisWeb.Application.Communication.ModelCatalog.Requests;

namespace IrisWeb.Application.Validators.ModelCatalog;

public sealed partial class CompleteUploadRequestValidator : AbstractValidator<CompleteUploadRequest>
{
    public CompleteUploadRequestValidator()
    {
        RuleFor(x => x.Size).GreaterThan(0);

        RuleFor(x => x.Sha256).Must(value => value is not null && Sha256Regex().IsMatch(value))
            .WithMessage("Sha256 deve conter 64 caracteres hexadecimais.");

        RuleFor(x => x.Manifest).Must(value => value.ValueKind == JsonValueKind.Object)
            .WithMessage("Manifest deve ser um objeto JSON.");
    }

    [GeneratedRegex("^[a-fA-F0-9]{64}$")]
    private static partial Regex Sha256Regex();
}
