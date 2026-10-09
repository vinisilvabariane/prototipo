using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.UseCases.FaceRecognition;
using IrisWeb.Application.Validators.FaceRecognition;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Rules;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class EnrollFaceBatchUseCaseTests
{
    private static readonly FaceMatchingPolicy Matching = new("v1", 2, 0.85f, 0.12f);

    private static readonly float[] Front = [1f, 0f, 0f];
    private static readonly float[] AlmostFront = [0.99f, 0.141f, 0f];
    private static readonly float[] Tilted = [0.8f, 0.6f, 0f];
    private static readonly float[] Side = [0f, 1f, 0f];
    private static readonly float[] Up = [0f, 0f, 1f];

    private readonly FakeFaceEmbeddingRepository _embeddings = new();
    private readonly FakeEnrollmentConfirmationRepository _confirmations = new();

    private EnrollFaceBatchUseCase UseCase(string? actor = "lider") =>
        new(_embeddings, _confirmations, Matching, EnrollmentScreeningPolicy.Default, new FakeCurrentActor(actor));

    private static EnrollFaceBatchRequest Request(string registration, bool confirm, params float[][] batch) =>
        new(registration, batch, "v1", confirm);

    [Fact]
    public async Task EmptyBase_EnrollsTheWholeBatch()
    {
        var result = await UseCase().ExecuteAsync(Request(" 16271 ", false, Front, AlmostFront), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.Enrolled, result.Value.Outcome);
        Assert.Equal(2, result.Value.Saved);
        Assert.All(_embeddings.Stored, embedding => Assert.Equal("16271", embedding.Registration));
        Assert.Equal(2, _embeddings.Stored.Count);
    }

    [Fact]
    public async Task PhotosThatDisagree_AreRefusedAsInconsistent()
    {
        var result = await UseCase().ExecuteAsync(Request("16271", true, Front, Side), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.Inconsistent, result.Value.Outcome);
        Assert.True(result.Value.Spread > 0.7f);
        Assert.Empty(_embeddings.Stored);
    }

    [Fact]
    public async Task FaceOfAnotherRegistration_IsBlockedEvenWithConfirmation()
    {
        _embeddings.Seed("12277", Front);
        _embeddings.Seed("12277", Front);

        var result = await UseCase().ExecuteAsync(Request("16271", true, AlmostFront), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.SamePerson, result.Value.Outcome);
        Assert.Equal("12277", result.Value.SimilarRegistration);
        Assert.Equal(2, _embeddings.Stored.Count);
        Assert.Empty(_confirmations.Stored);
    }

    [Fact]
    public async Task SimilarFace_AsksForConfirmationAndSavesNothing()
    {
        _embeddings.Seed("12277", Front);

        var result = await UseCase().ExecuteAsync(Request("16271", false, Tilted), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.Similar, result.Value.Outcome);
        Assert.Equal("12277", result.Value.SimilarRegistration);
        Assert.InRange(result.Value.Distance!.Value, 0.5f, 1.0f);
        Assert.Single(_embeddings.Stored);
    }

    [Fact]
    public async Task SimilarFace_Confirmed_EnrollsAndRecordsWhoConfirmed()
    {
        _embeddings.Seed("12277", Front);

        var result = await UseCase("lider-123").ExecuteAsync(Request("16271", true, Tilted), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.Enrolled, result.Value.Outcome);
        Assert.Equal(2, _embeddings.Stored.Count);
        var confirmation = Assert.Single(_confirmations.Stored);
        Assert.Equal("16271", confirmation.Registration);
        Assert.Equal("12277", confirmation.SimilarRegistration);
        Assert.Equal("lider-123", confirmation.ConfirmedBy);
    }

    [Fact]
    public async Task SimilarFace_ConfirmedWithoutActor_Fails()
    {
        _embeddings.Seed("12277", Front);

        var result = await UseCase(actor: null).ExecuteAsync(Request("16271", true, Tilted), CancellationToken.None);

        Assert.Equal(FaceRecognitionErrors.ConfirmationWithoutActor, result.Errors.Single().Message);
        Assert.Single(_embeddings.Stored);
    }

    [Fact]
    public async Task RegistrationWithAnotherFace_IsRefused()
    {
        _embeddings.Seed("16271", Front);
        _embeddings.Seed("16271", Front);

        var result = await UseCase().ExecuteAsync(Request("16271", true, Up), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.RegistrationMismatch, result.Value.Outcome);
        Assert.Equal(2, _embeddings.Stored.Count);
    }

    [Fact]
    public async Task MoreSamplesOfTheSamePerson_AreAccepted()
    {
        _embeddings.Seed("16271", Front);

        var result = await UseCase().ExecuteAsync(Request("16271", false, AlmostFront), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.Enrolled, result.Value.Outcome);
        Assert.Equal(2, _embeddings.Stored.Count);
    }

    [Fact]
    public async Task DistantFromEveryone_EnrollsWithoutConfirmation()
    {
        _embeddings.Seed("12277", Side);

        var result = await UseCase().ExecuteAsync(Request("16271", false, Front), CancellationToken.None);

        Assert.Equal(EnrollFaceBatchUseCase.Enrolled, result.Value.Outcome);
        Assert.Empty(_confirmations.Stored);
    }

    [Fact]
    public void Validator_RejectsMixedDimensionsAndEmptyBatch()
    {
        var validator = new EnrollFaceBatchRequestValidator();

        Assert.False(validator.Validate(new EnrollFaceBatchRequest("16271", [[1f, 0f], [1f, 0f, 0f]], "v1")).IsValid);
        Assert.False(validator.Validate(new EnrollFaceBatchRequest("16271", [], "v1")).IsValid);
        Assert.True(validator.Validate(new EnrollFaceBatchRequest("16271", [[1f, 0f, 0f]], "v1")).IsValid);
    }
}
