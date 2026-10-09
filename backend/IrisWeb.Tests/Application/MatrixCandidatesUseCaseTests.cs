using IrisWeb.Domain.Rules;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class MatrixCandidatesUseCaseTests
{
    private static readonly FaceMatchingPolicy Policy = new("v1", 2, 0.85f, 0.12f);

    private readonly FakeFaceEmbeddingRepository _embeddings = new();

    private static MatrixStationsQuery Query(string shift = "1º turno") =>
        new() { Line = " N111 ", Product = "621075005968", Shift = shift };

    private static FakeSyncMatrixClient Sync(int? activityId = 429) =>
        new() { Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA", activityId: activityId)]) };

    private static SyncRecommendedCollaborator Recommended(int registration, int position, int group = 1) =>
        new(registration * 10, $"OPERADOR {registration}", registration, "OPERADOR", "Qualificado", 80, position, group, true, 0, null);

    [Fact]
    public async Task Recommendations_UseTheShiftIdAndTheIdsFromTheLayout()
    {
        var sync = Sync();
        sync.Recommended.Add(Recommended(222, position: 2));
        sync.Recommended.Add(Recommended(111, position: 1));
        _embeddings.Seed("111", [1f, 0f, 0f]);

        var result = await new ListStationRecommendationsUseCase(sync, _embeddings, Policy).ExecuteAsync(14, Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((14, 35, 429, "621075005968", 1), sync.LastRecommendationQuery);
        Assert.Equal([111, 222], result.Value.Select(candidate => candidate.Registration));
        Assert.Equal("Qualificado", result.Value[0].Level);
        Assert.True(result.Value[0].HasValidPostTraining);
        Assert.True(result.Value[0].HasFaceEnrollment);
        Assert.False(result.Value[1].HasFaceEnrollment);
    }

    [Fact]
    public async Task Recommendations_UnknownShift_Fails()
    {
        var result = await new ListStationRecommendationsUseCase(Sync(), _embeddings, Policy).ExecuteAsync(14, Query("3º TURNO"), CancellationToken.None);

        Assert.Equal(MatrixErrors.ShiftNotFound("3º TURNO"), result.Errors.Single().Message);
    }

    [Fact]
    public async Task Recommendations_StationWithoutActivity_Fails()
    {
        var result = await new ListStationRecommendationsUseCase(Sync(activityId: null), _embeddings, Policy).ExecuteAsync(14, Query(), CancellationToken.None);

        Assert.Equal(MatrixErrors.StationWithoutActivity(14), result.Errors.Single().Message);
    }

    [Fact]
    public async Task Recommendations_StationMissingFromLayout_Fails()
    {
        var result = await new ListStationRecommendationsUseCase(Sync(), _embeddings, Policy).ExecuteAsync(99, Query(), CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Lookup_ReturnsTheLevelOnTheStationActivity()
    {
        var sync = Sync();
        sync.ActivityCollaborators[16271] = new SyncCollaboratorInformation(49560, " FULANO ", "Em treinamento", "OPERADOR", 40);

        var result = await new LookupStationCandidateUseCase(sync, _embeddings, Policy).ExecuteAsync(14, 16271, Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(16271, result.Value.Registration);
        Assert.Equal("FULANO", result.Value.Name);
        Assert.Equal("Em treinamento", result.Value.Level);
        Assert.False(result.Value.HasFaceEnrollment);
        Assert.Equal((16271, 429, "621075005968", "1º turno", 14, 35), sync.LastCandidateQuery);
    }

    [Fact]
    public async Task Lookup_NotFoundInSync_PassesTheRefusalThrough()
    {
        var result = await new LookupStationCandidateUseCase(Sync(), _embeddings, Policy).ExecuteAsync(14, 999, Query(), CancellationToken.None);

        Assert.Equal("Colaborador não encontrado", result.Errors.Single().Message);
    }
}
