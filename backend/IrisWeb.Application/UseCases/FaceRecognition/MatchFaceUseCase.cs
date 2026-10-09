using IrisWeb.Domain.Rules;
using FluentResults;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Communication.FaceRecognition.Responses;
using IrisWeb.Application.Interfaces.FaceRecognition;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.FaceRecognition;

public sealed class MatchFaceUseCase(
    IFaceEmbeddingRepository repository,
    FaceMatchingPolicy policy) : IMatchFaceUseCase
{
    public async Task<Result<MatchFaceResponse>> ExecuteAsync(
        MatchFaceRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Embedding.Length == 0)
            return Result.Fail(FaceRecognitionErrors.EmptyEmbedding);

        var modelVersion = policy.ResolveModelVersion(request.ModelVersion);

        var enrolled = await repository.ListByModelVersionAsync(modelVersion, cancellationToken);

        var topK = Math.Max(1, policy.MatchTopK);

        var scored = enrolled
            .Where(embedding => embedding.IsComparableTo(request.Embedding))
            .GroupBy(embedding => embedding.Registration)
            .Select(group => (
                Registration: group.Key,
                Score: group
                    .Select(embedding => embedding.DistanceTo(request.Embedding))
                    .OrderBy(distance => distance)
                    .Take(topK)
                    .Average()))
            .OrderBy(item => item.Score)
            .ToList();

        if (scored.Count == 0)
            return Result.Ok(new MatchFaceResponse(FaceRecognitionErrors.UnknownLabel, null, null));

        var (topRegistration, topDistance) = scored[0];
        var secondDistance = scored.Count > 1 ? scored[1].Score : float.PositiveInfinity;

        if (topDistance > policy.DistanceThreshold)
            return Result.Ok(new MatchFaceResponse(FaceRecognitionErrors.UnknownLabel, null, topDistance, topRegistration));

        if (secondDistance - topDistance < policy.DistanceMargin)
            return Result.Ok(new MatchFaceResponse(FaceRecognitionErrors.AmbiguousLabel, null, topDistance, topRegistration));

        return Result.Ok(new MatchFaceResponse(topRegistration, topRegistration, topDistance, topRegistration));
    }
}
