using IrisWeb.Domain.Rules;
using FluentValidation;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.FaceRecognition;

public sealed class EnrollFaceRequestValidator : AbstractValidator<EnrollFaceRequest>
{
    public EnrollFaceRequestValidator()
    {
        RuleFor(x => x.Registration)
            .NotEmpty().WithMessage(FaceRecognitionErrors.EmptyRegistration)
            .MaximumLength(StationNamingPolicy.MaxRegistrationLength);

        RuleFor(x => x.Embedding).NotEmpty().WithMessage(FaceRecognitionErrors.EmptyEmbedding);
        RuleFor(x => x.ModelVersion).NotEmpty();
    }
}
