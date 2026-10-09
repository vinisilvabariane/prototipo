using IrisWeb.Domain.Rules;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Application.Validators.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class MatrixAllocationUseCaseTests
{
    private static AllocateMatrixStationRequest Request(int stationId = 14, string registration = "16271") =>
        new(" N111 ", "621075005968", "1º TURNO", stationId, registration);

    private static readonly FaceMatchingPolicy Policy = new("v1", 2, 0.85f, 0.12f);

    private static FakeFaceEmbeddingRepository Enrolled(params string[] registrations)
    {
        var embeddings = new FakeFaceEmbeddingRepository();
        foreach (var registration in registrations.Length == 0 ? ["16271"] : registrations)
            embeddings.Seed(registration, [1f, 0f, 0f]);
        return embeddings;
    }

    [Fact]
    public async Task Allocate_WithoutFaceEnrollment_IsRefusedBeforeCallingSync()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA", activityId: 429));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled("99999"), Policy);

        var result = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Assert.Equal(MatrixErrors.NoFaceEnrollment(16271), result.Errors.Single().Message);
        Assert.Empty(sync.Setups);
    }

    [Fact]
    public async Task Allocate_EnrollmentWithLeadingZeros_StillCounts()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA", activityId: 429));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled("0016271"), Policy);

        var result = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private static FakeSyncMatrixClient SyncWithCollaborator(params SyncLayoutStation[] stations)
    {
        var sync = new FakeSyncMatrixClient { Layout = new SyncLayout(35, stations) };
        sync.Collaborators[16271] = new SyncCollaborator(49560, 16271, "FULANO");
        return sync;
    }

    [Fact]
    public async Task Allocate_CallsSyncSetupWithIdsTakenFromTheLayout()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA", activityId: 429));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var setup = Assert.Single(sync.Setups);
        Assert.Equal(new SyncSetupCollaborator(49560, 35, 14, 429, "1º TURNO", "621075005968"), setup);
        Assert.Equal(("N111", "621075005968", "1º TURNO"), sync.LastLayoutQuery);
    }

    [Fact]
    public async Task Allocate_ReturnsTheStationAsTheSyncReportsItAfterwards()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271, level: "Qualificado"));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Assert.Equal(16271, result.Value.Allocated!.Registration);
        Assert.Equal(220, result.Value.Width);
        Assert.Equal(140, result.Value.Height);
    }

    [Fact]
    public async Task Allocate_StationWithoutActivity_FailsWithoutCallingSetup()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA", activityId: null));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(MatrixErrors.StationWithoutActivity(14), result.Errors.Single().Message);
        Assert.Empty(sync.Setups);
    }

    [Fact]
    public async Task Allocate_StationNotInLayout_Fails()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA"));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(stationId: 99), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(sync.Setups);
    }

    [Fact]
    public async Task Allocate_NonNumericRegistration_FailsBeforeCallingTheSync()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA"));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(registration: "AB12"), CancellationToken.None);

        Assert.Equal(MatrixErrors.RegistrationNotNumeric, result.Errors.Single().Message);
        Assert.Null(sync.LastLayoutQuery);
    }

    [Fact]
    public async Task Allocate_UnknownRegistrationInTheSync_IsBusinessError()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA"));
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(registration: "999"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<UpstreamGatewayError>());
        Assert.Empty(sync.Setups);
    }

    [Fact]
    public async Task Allocate_SyncRefusal_KeepsTheSyncMessage()
    {
        var sync = SyncWithCollaborator(FakeSyncMatrixClient.Station(14, "PRENSA"));
        sync.SetupFailure = new SyncMatrixException("Sync respondeu 400.", 400, "Colaborador FULANO não passou pela catraca hoje.");
        var useCase = new AllocateMatrixStationUseCase(sync, Enrolled(), Policy);

        var result = await useCase.ExecuteAsync(Request(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<UpstreamGatewayError>());
        Assert.Equal("Colaborador FULANO não passou pela catraca hoje.", result.Errors.Single().Message);
    }

    [Fact]
    public void Request_RejectsBlankRegistration()
    {
        var validator = new AllocateMatrixStationRequestValidator();

        var result = validator.Validate(new AllocateMatrixStationRequest("N111", "621075005968", "1º TURNO", 14, " "));

        Assert.False(result.IsValid);
    }
}
