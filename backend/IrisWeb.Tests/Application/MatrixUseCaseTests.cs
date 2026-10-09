using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Application.Validators.Matrix;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class MatrixUseCaseTests
{
    private const string Device = "a1b2c3d4e5f60718";

    private static FakeSyncMatrixClient SyncWith(params SyncLayoutStation[] stations) =>
        new() { Layout = new SyncLayout(35, stations) };

    private static FakeDeviceMatrixBindingRepository BoundTo(int stationId)
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        bindings.Seed(DeviceMatrixBinding.Create(Device, "N111", "621075005968", "1º TURNO", stationId, "PRENSA"));
        return bindings;
    }

    [Fact]
    public async Task Allocation_ReturnsWhoIsAllocatedOnTheBoundStation()
    {
        var sync = SyncWith(
            FakeSyncMatrixClient.Station(13, "VAZIO"),
            FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271, level: "Qualificado"));
        var useCase = new GetDeviceAllocationUseCase(BoundTo(14), sync);

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(("N111", "621075005968", "1º TURNO"), sync.LastLayoutQuery);
        Assert.Equal(14, result.Value.Binding.StationId);
        Assert.NotNull(result.Value.Allocated);
        Assert.Equal(16271, result.Value.Allocated!.Registration);
        Assert.Equal("Qualificado", result.Value.Allocated.Level);
    }

    [Fact]
    public async Task Allocation_FreeStation_HasNoAllocated()
    {
        var useCase = new GetDeviceAllocationUseCase(BoundTo(13), SyncWith(FakeSyncMatrixClient.Station(13, "VAZIO")));

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Allocated);
    }

    [Fact]
    public async Task Allocation_UnboundDevice_Fails()
    {
        var useCase = new GetDeviceAllocationUseCase(new FakeDeviceMatrixBindingRepository(), SyncWith());

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(MatrixErrors.DeviceNotBound, result.Errors.Single().Message);
    }

    [Fact]
    public async Task Allocation_StationRemovedFromLayout_Fails()
    {
        var useCase = new GetDeviceAllocationUseCase(BoundTo(99), SyncWith(FakeSyncMatrixClient.Station(14, "PRENSA")));

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("#99", result.Errors.Single().Message);
    }

    [Fact]
    public async Task Allocation_SyncRefusal_IsBusinessError_NotGateway()
    {
        var sync = SyncWith();
        sync.Failure = new SyncMatrixException("Sync respondeu 404.", 404, "Nenhum layout encontrado.");
        var useCase = new GetDeviceAllocationUseCase(BoundTo(14), sync);

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<UpstreamGatewayError>());
        Assert.Equal("Nenhum layout encontrado.", result.Errors.Single().Message);
    }

    [Fact]
    public async Task Allocation_SyncDown_IsGatewayError()
    {
        var sync = SyncWith();
        sync.Failure = new HttpRequestException("connection refused");
        var useCase = new GetDeviceAllocationUseCase(BoundTo(14), sync);

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.HasError<UpstreamGatewayError>());
    }

    [Fact]
    public async Task Bind_StoresStationNameFromTheLayout()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        var useCase = new BindDeviceMatrixUseCase(bindings, SyncWith(FakeSyncMatrixClient.Station(14, "  PRENSA BATERIA ")));

        var result = await useCase.ExecuteAsync(
            $" {Device} ",
            new BindDeviceMatrixRequest(" N111 ", "621075005968", "1º TURNO", 14),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(bindings.Stored);
        Assert.Equal(Device, stored.Id);
        Assert.Equal("N111", stored.Line);
        Assert.Equal("PRENSA BATERIA", stored.StationName);
    }

    [Fact]
    public async Task Bind_SameDeviceTwice_UpdatesTheSameRow()
    {
        var bindings = BoundTo(14);
        var useCase = new BindDeviceMatrixUseCase(bindings, SyncWith(FakeSyncMatrixClient.Station(20, "ESPUMA")));

        var result = await useCase.ExecuteAsync(
            Device,
            new BindDeviceMatrixRequest("N111", "621075005968", "2º TURNO", 20),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(bindings.Stored);
        Assert.Equal(20, stored.StationId);
        Assert.Equal("2º TURNO", stored.Shift);
    }

    [Fact]
    public async Task Bind_StationNotInLayout_FailsWithoutPersisting()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        var useCase = new BindDeviceMatrixUseCase(bindings, SyncWith(FakeSyncMatrixClient.Station(14, "PRENSA")));

        var result = await useCase.ExecuteAsync(
            Device,
            new BindDeviceMatrixRequest("N111", "621075005968", "1º TURNO", 77),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(bindings.Stored);
    }

    [Fact]
    public async Task Bind_BlankDevice_Fails()
    {
        var useCase = new BindDeviceMatrixUseCase(new FakeDeviceMatrixBindingRepository(), SyncWith());

        var result = await useCase.ExecuteAsync(
            "  ",
            new BindDeviceMatrixRequest("N111", "X", "1º TURNO", 14),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(MatrixErrors.DeviceRequired, result.Errors.Single().Message);
    }

    [Fact]
    public async Task Unbind_IsIdempotent()
    {
        var bindings = BoundTo(14);
        var useCase = new UnbindDeviceMatrixUseCase(bindings);

        Assert.True((await useCase.ExecuteAsync(Device, CancellationToken.None)).IsSuccess);
        Assert.Empty(bindings.Stored);
        Assert.True((await useCase.ExecuteAsync(Device, CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task GetDevice_Unbound_ReturnsNullNotFailure()
    {
        var useCase = new GetDeviceMatrixUseCase(new FakeDeviceMatrixBindingRepository());

        var result = await useCase.ExecuteAsync(Device, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task Stations_AreOrderedAndCarryTheOccupant()
    {
        var sync = SyncWith(
            FakeSyncMatrixClient.Station(20, "ESPUMA"),
            FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271, level: "Sem treinamento"));
        var useCase = new ListMatrixStationsUseCase(sync);

        var result = await useCase.ExecuteAsync(
            new MatrixStationsQuery { Line = "N111", Product = "621075005968", Shift = "1º TURNO" },
            CancellationToken.None);

        Assert.Equal([14, 20], result.Value.Select(station => station.Id));
        Assert.Equal(16271, result.Value[0].Allocated!.Registration);
        Assert.Null(result.Value[1].Allocated);
    }

    [Fact]
    public void BindRequest_RejectsMissingStation()
    {
        var validator = new BindDeviceMatrixRequestValidator();

        var result = validator.Validate(new BindDeviceMatrixRequest("N111", "621075005968", "1º TURNO", 0));

        Assert.False(result.IsValid);
    }
}
