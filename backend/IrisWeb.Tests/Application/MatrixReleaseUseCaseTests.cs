using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class MatrixReleaseUseCaseTests
{
    private static MatrixStationsQuery Query() =>
        new() { Line = " N111 ", Product = "621075005968", Shift = "1º TURNO" };

    [Fact]
    public async Task Release_RemovesOnlyThisStationWithTheIdsFromTheLayout()
    {
        var sync = new FakeSyncMatrixClient
        {
            Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271, level: "Qualificado")]),
        };

        var result = await new ReleaseMatrixStationUseCase(sync).ExecuteAsync(14, Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new SyncRemoveCollaborator(1400, 35, 14), Assert.Single(sync.Removals));
        Assert.Equal(("N111", "621075005968", "1º TURNO"), sync.LastLayoutQuery);
    }

    [Fact]
    public async Task Release_EmptyStation_FailsWithoutCallingSync()
    {
        var sync = new FakeSyncMatrixClient { Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA")]) };

        var result = await new ReleaseMatrixStationUseCase(sync).ExecuteAsync(14, Query(), CancellationToken.None);

        Assert.Equal(MatrixErrors.StationAlreadyEmpty(14), result.Errors.Single().Message);
        Assert.Empty(sync.Removals);
    }

    [Fact]
    public async Task Release_StationMissingFromLayout_Fails()
    {
        var sync = new FakeSyncMatrixClient { Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271)]) };

        var result = await new ReleaseMatrixStationUseCase(sync).ExecuteAsync(99, Query(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(sync.Removals);
    }

    [Fact]
    public async Task Release_SyncRefusal_PassesTheMessageThrough()
    {
        var sync = new FakeSyncMatrixClient
        {
            Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271)]),
            SetupFailure = new SyncMatrixException("Sync respondeu 400.", 400, "Colaborador nao esta configurado para essa linha"),
        };

        var result = await new ReleaseMatrixStationUseCase(sync).ExecuteAsync(14, Query(), CancellationToken.None);

        Assert.Equal("Colaborador nao esta configurado para essa linha", result.Errors.Single().Message);
    }
}
