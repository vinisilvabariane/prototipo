using IrisWeb.Domain.Rules;
using FluentResults;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Communication.FaceRecognition.Responses;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.FaceRecognition;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.FaceRecognition;

public sealed class EnrollFaceBatchUseCase(
    IFaceEmbeddingRepository embeddings,
    IEnrollmentConfirmationRepository confirmations,
    FaceMatchingPolicy matching,
    EnrollmentScreeningPolicy screening,
    ICurrentActor currentActor) : IEnrollFaceBatchUseCase
{
    public const string Enrolled = "enrolled";
    public const string Similar = "similar";
    public const string SamePerson = "same_person";
    public const string Inconsistent = "inconsistent";
    public const string RegistrationMismatch = "registration_mismatch";

    public async Task<Result<EnrollFaceBatchResponse>> ExecuteAsync(
        EnrollFaceBatchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Registration))
            return Result.Fail(FaceRecognitionErrors.EmptyRegistration);

        if (request.Embeddings.Length == 0 || request.Embeddings.Any(sample => sample is not { Length: > 0 }))
            return Result.Fail(FaceRecognitionErrors.EmptyEmbedding);

        var registration = request.Registration.Trim();
        var modelVersion = matching.ResolveModelVersion(request.ModelVersion);
        var enrolled = await embeddings.ListByModelVersionAsync(modelVersion, cancellationToken);

        var verdict = EnrollmentScreening.Evaluate(registration, request.Embeddings, enrolled, matching, screening);

        switch (verdict.Kind)
        {
            case EnrollmentVerdictKind.Inconsistent:
                return Refused(Inconsistent, verdict);
            case EnrollmentVerdictKind.SamePerson:
                return Refused(SamePerson, verdict);
            case EnrollmentVerdictKind.RegistrationMismatch:
                return Refused(RegistrationMismatch, verdict);
            case EnrollmentVerdictKind.Similar when !request.ConfirmSimilar:
                return Refused(Similar, verdict);
        }

        EnrollmentConfirmation? confirmation = null;
        if (verdict.Kind == EnrollmentVerdictKind.Similar)
        {
            if (string.IsNullOrWhiteSpace(currentActor.Registration))
                return Result.Fail(FaceRecognitionErrors.ConfirmationWithoutActor);

            confirmation = EnrollmentConfirmation.Create(
                registration,
                verdict.ClosestRegistration!,
                verdict.ClosestDistance!.Value,
                currentActor.Registration);
            await confirmations.AddAsync(confirmation, cancellationToken);
        }

        await embeddings.AddRangeAsync(
            request.Embeddings.Select(sample => FaceEmbedding.Create(registration, sample, modelVersion)),
            cancellationToken);

        if (await embeddings.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(FaceRecognitionErrors.NothingPersisted);

        if (confirmation is not null)
            await confirmations.SaveChangesAsync(cancellationToken);

        return Result.Ok(new EnrollFaceBatchResponse(
            Enrolled,
            request.Embeddings.Length,
            verdict.ClosestRegistration,
            verdict.ClosestDistance,
            verdict.Spread));
    }

    private static Result<EnrollFaceBatchResponse> Refused(string outcome, EnrollmentVerdict verdict) =>
        Result.Ok(new EnrollFaceBatchResponse(outcome, 0, verdict.ClosestRegistration, verdict.ClosestDistance, verdict.Spread));
}
