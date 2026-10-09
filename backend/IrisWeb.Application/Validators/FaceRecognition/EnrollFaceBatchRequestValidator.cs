using IrisWeb.Domain.Rules;
using FluentValidation;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.Validators.FaceRecognition;

public sealed class EnrollFaceBatchRequestValidator : AbstractValidator<EnrollFaceBatchRequest>
{
    public const int MaxEmbeddings = 20;

    public EnrollFaceBatchRequestValidator()
    {
        RuleFor(x => x.Registration)
            .NotEmpty().WithMessage(FaceRecognitionErrors.EmptyRegistration)
            .MaximumLength(StationNamingPolicy.MaxRegistrationLength);

        RuleFor(x => x.Embeddings)
            .NotEmpty().WithMessage(FaceRecognitionErrors.EmptyEmbedding)
            .Must(batch => batch.Length <= MaxEmbeddings).WithMessage(FaceRecognitionErrors.TooManyEmbeddings)
            .Must(batch => batch.All(sample => sample is { Length: > 0 })).WithMessage(FaceRecognitionErrors.EmptyEmbedding)
            .Must(batch => batch.Select(sample => sample?.Length ?? 0).Distinct().Count() <= 1)
            .WithMessage(FaceRecognitionErrors.MixedEmbeddingDimensions);

        RuleFor(x => x.ModelVersion).NotEmpty();
    }
}
