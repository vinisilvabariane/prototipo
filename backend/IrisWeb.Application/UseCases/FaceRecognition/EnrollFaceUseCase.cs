using IrisWeb.Domain.Rules;
using FluentResults;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Interfaces.FaceRecognition;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.FaceRecognition;

public sealed class EnrollFaceUseCase(
    IFaceEmbeddingRepository repository,
    FaceMatchingPolicy policy) : IEnrollFaceUseCase
{
    public async Task<Result> ExecuteAsync(EnrollFaceRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Registration))
            return Result.Fail(FaceRecognitionErrors.EmptyRegistration);

        if (request.Embedding.Length == 0)
            return Result.Fail(FaceRecognitionErrors.EmptyEmbedding);

        var modelVersion = policy.ResolveModelVersion(request.ModelVersion);

        var embedding = FaceEmbedding.Create(request.Registration, request.Embedding, modelVersion);

        await repository.AddAsync(embedding, cancellationToken);

        if (await repository.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(FaceRecognitionErrors.NothingPersisted);

        return Result.Ok();
    }
}
