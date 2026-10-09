using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Application.UseCases.Stations;
using IrisWeb.Application.Validators.Stations;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class MatrixUnknownUseCaseTests
{
    private const string Device = "a1b2c3d4e5f60718";

    private static FakeDeviceMatrixBindingRepository Bound()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        bindings.Seed(DeviceMatrixBinding.Create(Device, "n111", "621075005968", "1º TURNO", 14, "prensa bateria"));
        return bindings;
    }

    [Fact]
    public async Task OpenUnknown_UsesTheMatrixStationOfTheDevice()
    {
        var unknowns = new FakeStationUnknownRepository();
        var useCase = new OpenDeviceUnknownUseCase(Bound(), unknowns);

        var result = await useCase.ExecuteAsync(Device, new OpenDeviceUnknownRequest(8), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var unknown = Assert.Single(unknowns.Stored);
        Assert.Equal("#14 PRENSA BATERIA", unknown.Workstation);
        Assert.Equal("N111", unknown.Line);
        Assert.True(unknown.IsOpen);
        Assert.True((DateTime.Now - unknown.StartedAt).TotalSeconds >= 7);
        Assert.Equal(unknown.Id, result.Value.UnknownId);
    }

    [Fact]
    public async Task OpenUnknown_UnboundDevice_Fails()
    {
        var unknowns = new FakeStationUnknownRepository();
        var useCase = new OpenDeviceUnknownUseCase(new FakeDeviceMatrixBindingRepository(), unknowns);

        var result = await useCase.ExecuteAsync(Device, new OpenDeviceUnknownRequest(0), CancellationToken.None);

        Assert.Equal(MatrixErrors.DeviceNotBound, result.Errors.Single().Message);
        Assert.Empty(unknowns.Stored);
    }

    [Fact]
    public async Task CloseUnknown_StoresDurationAndTheClosestCandidate()
    {
        var unknowns = new FakeStationUnknownRepository();
        var open = new OpenDeviceUnknownUseCase(Bound(), unknowns);
        await open.ExecuteAsync(Device, new OpenDeviceUnknownRequest(120), CancellationToken.None);
        var unknown = unknowns.Stored.Single();

        var result = await new CloseUnknownUseCase(unknowns).ExecuteAsync(
            unknown.Id,
            new CloseUnknownRequest(20, " 16271 ", 0.91f),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(unknown.IsOpen);
        Assert.InRange(unknown.DurationSeconds!.Value, 99, 101);
        Assert.Equal("16271", unknown.CandidateRegistration);
        Assert.Equal(0.91f, unknown.CandidateDistance);
    }

    [Fact]
    public async Task CloseUnknown_WithoutCandidate_KeepsDiagnosticEmpty()
    {
        var unknowns = new FakeStationUnknownRepository();
        await new OpenDeviceUnknownUseCase(Bound(), unknowns).ExecuteAsync(Device, new OpenDeviceUnknownRequest(10), CancellationToken.None);
        var unknown = unknowns.Stored.Single();

        await new CloseUnknownUseCase(unknowns).ExecuteAsync(unknown.Id, new CloseUnknownRequest(0, null, null), CancellationToken.None);

        Assert.False(unknown.IsOpen);
        Assert.Null(unknown.CandidateRegistration);
        Assert.Null(unknown.CandidateDistance);
    }

    [Fact]
    public async Task CloseUnknown_AlreadyClosedOrMissing_StillSucceeds()
    {
        var result = await new CloseUnknownUseCase(new FakeStationUnknownRepository()).ExecuteAsync(
            42,
            new CloseUnknownRequest(0, null, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CloseRequest_RejectsNegativeDistance()
    {
        var validator = new CloseUnknownRequestValidator();

        Assert.False(validator.Validate(new CloseUnknownRequest(0, "16271", -0.1f)).IsValid);
    }
}
