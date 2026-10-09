using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Application.Validators.Matrix;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class MatrixPresenceUseCaseTests
{
    private const string Device = "a1b2c3d4e5f60718";

    private static FakeDeviceMatrixBindingRepository Bound()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        bindings.Seed(DeviceMatrixBinding.Create(Device, "n111", "621075005968", "1º TURNO", 14, "prensa bateria"));
        return bindings;
    }

    [Fact]
    public async Task OpenPresence_UsesTheMatrixStationOfTheDevice()
    {
        var presences = new FakeStationPresenceRepository();
        var useCase = new OpenDevicePresenceUseCase(Bound(), presences, new FakeSyncMatrixClient());

        var result = await useCase.ExecuteAsync(Device, new OpenDevicePresenceRequest(" 16271 "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var presence = Assert.Single(presences.Stored);
        Assert.Equal("16271", presence.Registration);
        Assert.Equal("#14 PRENSA BATERIA", presence.Workstation);
        Assert.Equal("N111", presence.Line);
        Assert.True(presence.IsOpen);
        Assert.Equal(presence.Id, result.Value.SessionId);
    }

    [Fact]
    public async Task OpenPresence_UnboundDevice_FailsWithoutPersisting()
    {
        var presences = new FakeStationPresenceRepository();
        var useCase = new OpenDevicePresenceUseCase(new FakeDeviceMatrixBindingRepository(), presences, new FakeSyncMatrixClient());

        var result = await useCase.ExecuteAsync(Device, new OpenDevicePresenceRequest("16271"), CancellationToken.None);

        Assert.Equal(MatrixErrors.DeviceNotBound, result.Errors.Single().Message);
        Assert.Empty(presences.Stored);
    }

    [Fact]
    public async Task OpenPresence_BlankRegistration_Fails()
    {
        var presences = new FakeStationPresenceRepository();
        var useCase = new OpenDevicePresenceUseCase(Bound(), presences, new FakeSyncMatrixClient());

        var result = await useCase.ExecuteAsync(Device, new OpenDevicePresenceRequest("   "), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(presences.Stored);
    }

    [Fact]
    public async Task OpenVacancy_UsesTheMatrixStationOfTheDevice()
    {
        var vacancies = new FakeStationVacancyRepository();
        var useCase = new OpenDeviceVacancyUseCase(Bound(), vacancies);

        var result = await useCase.ExecuteAsync(Device, new OpenDeviceVacancyRequest(30), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var vacancy = Assert.Single(vacancies.Stored);
        Assert.Equal("#14 PRENSA BATERIA", vacancy.Workstation);
        Assert.Equal("N111", vacancy.Line);
        Assert.True((DateTime.Now - vacancy.StartedAt).TotalSeconds >= 29);
    }

    [Fact]
    public async Task OpenVacancy_UnboundDevice_Fails()
    {
        var vacancies = new FakeStationVacancyRepository();
        var useCase = new OpenDeviceVacancyUseCase(new FakeDeviceMatrixBindingRepository(), vacancies);

        var result = await useCase.ExecuteAsync(Device, new OpenDeviceVacancyRequest(0), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Empty(vacancies.Stored);
    }

    [Fact]
    public void VacancyRequest_RejectsNegativeInterval()
    {
        var validator = new OpenDeviceVacancyRequestValidator();

        Assert.False(validator.Validate(new OpenDeviceVacancyRequest(-1)).IsValid);
    }

    [Fact]
    public async Task OpenPresence_ClosesStrayOpenPresenceAtSameStationFirst()
    {
        var presences = new FakeStationPresenceRepository();
        var stray = StationPresence.Restore(
            1, "99999", "#14 PRENSA BATERIA", "N111", DateTime.Now.AddDays(-2), null, null);
        presences.Seed(stray);

        var useCase = new OpenDevicePresenceUseCase(Bound(), presences, new FakeSyncMatrixClient());

        var result = await useCase.ExecuteAsync(Device, new OpenDevicePresenceRequest("16271"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(stray.IsOpen);
        Assert.NotNull(stray.DurationSeconds);
        var open = Assert.Single(presences.Stored, presence => presence.IsOpen);
        Assert.Equal("16271", open.Registration);
    }

    [Fact]
    public async Task OpenPresence_DoesNotTouchOpenPresenceAtOtherStation()
    {
        var presences = new FakeStationPresenceRepository();
        var otherStation = StationPresence.Restore(
            1, "99999", "#2 OUTRO POSTO", "N111", DateTime.Now.AddDays(-2), null, null);
        presences.Seed(otherStation);

        var useCase = new OpenDevicePresenceUseCase(Bound(), presences, new FakeSyncMatrixClient());

        var result = await useCase.ExecuteAsync(Device, new OpenDevicePresenceRequest("16271"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(otherStation.IsOpen);
    }

    [Fact]
    public async Task OpenVacancy_ClosesStrayOpenVacancyAtSameStationFirst()
    {
        var vacancies = new FakeStationVacancyRepository();
        var stray = StationVacancy.Restore(1, "#14 PRENSA BATERIA", "N111", DateTime.Now.AddDays(-2), null, null);
        vacancies.Seed(stray);

        var useCase = new OpenDeviceVacancyUseCase(Bound(), vacancies);

        var result = await useCase.ExecuteAsync(Device, new OpenDeviceVacancyRequest(0), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(stray.IsOpen);
        Assert.Single(vacancies.Stored, vacancy => vacancy.IsOpen);
    }

    [Fact]
    public async Task OpenUnknown_ClosesStrayOpenUnknownAtSameStationFirst()
    {
        var unknowns = new FakeStationUnknownRepository();
        var stray = StationUnknown.Restore(1, "#14 PRENSA BATERIA", "N111", DateTime.Now.AddDays(-2), null, null, null, null);
        unknowns.Seed(stray);

        var useCase = new OpenDeviceUnknownUseCase(Bound(), unknowns);

        var result = await useCase.ExecuteAsync(Device, new OpenDeviceUnknownRequest(0), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(stray.IsOpen);
        Assert.Single(unknowns.Stored, unknown => unknown.IsOpen);
    }
}
