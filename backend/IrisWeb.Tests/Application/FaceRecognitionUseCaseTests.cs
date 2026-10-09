using IrisWeb.Domain.Rules;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.UseCases.FaceRecognition;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class FaceRecognitionUseCaseTests
{
    [Fact]
    public async Task MatchAsync_EmptyEmbedding_Fails()
    {
        var useCase = CreateMatch(new FakeFaceEmbeddingRepository());
        var result = await useCase.ExecuteAsync(new MatchFaceRequest([], "v1"), CancellationToken.None);
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task MatchAsync_NoEnrolledEmbeddings_ReturnsUnknown()
    {
        var useCase = CreateMatch(new FakeFaceEmbeddingRepository());

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([1f, 0f, 0f], "v1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(FaceRecognitionErrors.UnknownLabel, result.Value.Label);
        Assert.Null(result.Value.Registration);
        Assert.Null(result.Value.Score);
    }

    [Fact]
    public async Task MatchAsync_ExactMatch_ReturnsRegistrationWithZeroScore()
    {
        var repository = new FakeFaceEmbeddingRepository();
        repository.Seed("12345", [1f, 0f, 0f]);
        var useCase = CreateMatch(repository);

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([1f, 0f, 0f], "v1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("12345", result.Value.Registration);
        Assert.Equal("12345", result.Value.Label);
        Assert.NotNull(result.Value.Score);
        Assert.Equal(0f, result.Value.Score!.Value, precision: 5);
    }

    [Fact]
    public async Task MatchAsync_DistanceAboveThreshold_ReturnsUnknown()
    {
        var repository = new FakeFaceEmbeddingRepository();
        repository.Seed("12345", [1f, 0f, 0f]);
        var useCase = CreateMatch(repository, distanceThreshold: 0.5f);

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([0f, 1f, 0f], "v1"), CancellationToken.None);

        Assert.Equal(FaceRecognitionErrors.UnknownLabel, result.Value.Label);
        Assert.Null(result.Value.Registration);
    }

    [Fact]
    public async Task MatchAsync_Refused_StillReportsTheClosestCandidate()
    {
        var repository = new FakeFaceEmbeddingRepository();
        repository.Seed("12345", [1f, 0f, 0f]);
        repository.Seed("99999", [0f, 0f, 5f]);
        var useCase = CreateMatch(repository, distanceThreshold: 0.5f);

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([0.6f, 0.6f, 0f], "v1"), CancellationToken.None);

        Assert.Equal(FaceRecognitionErrors.UnknownLabel, result.Value.Label);
        Assert.Null(result.Value.Registration);
        Assert.Equal("12345", result.Value.Candidate);
        Assert.NotNull(result.Value.Score);
    }

    [Fact]
    public async Task MatchAsync_TwoCandidatesTooClose_ReturnsAmbiguous()
    {
        var repository = new FakeFaceEmbeddingRepository();
        repository.Seed("AAA", [0f, 0f, 0f]);
        repository.Seed("BBB", [0.05f, 0f, 0f]);
        var useCase = CreateMatch(repository, matchTopK: 1, distanceMargin: 0.12f);

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([0.02f, 0f, 0f], "v1"), CancellationToken.None);

        Assert.Equal("indefinido", result.Value.Label);
        Assert.Null(result.Value.Registration);
    }

    [Fact]
    public async Task MatchAsync_AveragesTopKClosestSamplesPerRegistration_IgnoringOutlier()
    {
        var repository = new FakeFaceEmbeddingRepository();
        repository.Seed("777", [0f, 0f, 0f]);
        repository.Seed("777", [0.1f, 0f, 0f]);
        repository.Seed("777", [10f, 0f, 0f]);
        var useCase = CreateMatch(repository, matchTopK: 2);

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([0f, 0f, 0f], "v1"), CancellationToken.None);

        Assert.Equal("777", result.Value.Registration);
        Assert.True(result.Value.Score < 0.1f, $"score esperado < 0.1, veio {result.Value.Score}");
    }

    [Fact]
    public async Task MatchAsync_BlankModelVersion_FallsBackToConfigDefault()
    {
        var repository = new FakeFaceEmbeddingRepository();
        var useCase = CreateMatch(repository, modelVersion: "meu-modelo-default");

        await useCase.ExecuteAsync(new MatchFaceRequest([1f, 0f, 0f], ""), CancellationToken.None);

        Assert.Equal("meu-modelo-default", repository.LastRequestedModelVersion);
    }

    [Fact]
    public async Task MatchAsync_IgnoresEnrolledEmbeddingsWithMismatchedDimension()
    {
        var repository = new FakeFaceEmbeddingRepository();
        repository.Seed("shortvec", [1f, 0f]);
        var useCase = CreateMatch(repository);

        var result = await useCase.ExecuteAsync(new MatchFaceRequest([1f, 0f, 0f], "v1"), CancellationToken.None);

        Assert.Equal(FaceRecognitionErrors.UnknownLabel, result.Value.Label);
    }

    [Fact]
    public async Task EnrollAsync_EmptyRegistration_Fails()
    {
        var useCase = CreateEnroll(new FakeFaceEmbeddingRepository());

        var result = await useCase.ExecuteAsync(new EnrollFaceRequest("   ", [1f, 0f, 0f], "v1"), CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task EnrollAsync_EmptyEmbedding_Fails()
    {
        var useCase = CreateEnroll(new FakeFaceEmbeddingRepository());

        var result = await useCase.ExecuteAsync(new EnrollFaceRequest("12345", [], "v1"), CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task EnrollAsync_Valid_SavesTrimmedRegistrationToRepository()
    {
        var repository = new FakeFaceEmbeddingRepository();
        var useCase = CreateEnroll(repository);
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        var result = await useCase.ExecuteAsync(new EnrollFaceRequest("  12345  ", embedding, "v1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(repository.Stored);
        Assert.Equal("12345", saved.Registration);
        Assert.Equal(embedding, saved.Values);
        Assert.Equal("v1", saved.ModelVersion);
        Assert.Equal(embedding.Length, saved.Dimension);
    }

    private static MatchFaceUseCase CreateMatch(
        IFaceEmbeddingRepository repository,
        float distanceThreshold = 1.0f,
        float distanceMargin = 0.12f,
        int matchTopK = 2,
        string modelVersion = "inception_resnet_v1_aligned") =>
        new(repository, CreatePolicy(distanceThreshold, distanceMargin, matchTopK, modelVersion));

    private static EnrollFaceUseCase CreateEnroll(
        IFaceEmbeddingRepository repository,
        string modelVersion = "inception_resnet_v1_aligned") =>
        new(repository, CreatePolicy(1.0f, 0.12f, 2, modelVersion));

    private static FaceMatchingPolicy CreatePolicy(
        float distanceThreshold,
        float distanceMargin,
        int matchTopK,
        string modelVersion) =>
        new(modelVersion, matchTopK, distanceThreshold, distanceMargin);
}
