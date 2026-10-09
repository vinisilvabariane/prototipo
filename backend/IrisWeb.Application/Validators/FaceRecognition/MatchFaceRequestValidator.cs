using FluentValidation;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.FaceRecognition;

public sealed class MatchFaceRequestValidator : AbstractValidator<MatchFaceRequest>
{
    public MatchFaceRequestValidator()
    {
        RuleFor(x => x.Embedding).NotEmpty().WithMessage(FaceRecognitionErrors.EmptyEmbedding);
        RuleFor(x => x.ModelVersion).NotEmpty();
    }
}
