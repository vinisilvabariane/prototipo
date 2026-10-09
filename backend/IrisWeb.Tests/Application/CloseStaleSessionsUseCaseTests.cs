using IrisWeb.Application.UseCases.Stations;
using IrisWeb.Domain.Entities;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class CloseStaleSessionsUseCaseTests
{
    private static readonly TimeSpan MaxOpenAge = TimeSpan.FromHours(48);

    [Fact]
    public async Task ExecuteAsync_ClosesPresenceOpenPastTheCutoff()
    {
        var presences = new FakeStationPresenceRepository();
        var stale = StationPresence.Restore(
            1, "12345", "#14 PRENSA BATERIA", "N111", DateTime.Now.AddHours(-72), null, null);
        presences.Seed(stale);

        var useCase = new CloseStaleSessionsUseCase(
            presences, new FakeStationVacancyRepository(), new FakeStationUnknownRepository());

        var result = await useCase.ExecuteAsync(MaxOpenAge, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.False(stale.IsOpen);
        Assert.NotNull(stale.DurationSeconds);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesRecentlyOpenedPresenceAlone()
    {
        var presences = new FakeStationPresenceRepository();
        var recent = StationPresence.Restore(
            1, "12345", "#14 PRENSA BATERIA", "N111", DateTime.Now.AddHours(-1), null, null);
        presences.Seed(recent);

        var useCase = new CloseStaleSessionsUseCase(
            presences, new FakeStationVacancyRepository(), new FakeStationUnknownRepository());

        var result = await useCase.ExecuteAsync(MaxOpenAge, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.True(recent.IsOpen);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesAlreadyClosedPresenceAlone()
    {
        var presences = new FakeStationPresenceRepository();
        var closed = StationPresence.Restore(
            1, "12345", "#14 PRENSA BATERIA", "N111",
            DateTime.Now.AddHours(-72), DateTime.Now.AddHours(-71), 3600);
        presences.Seed(closed);

        var useCase = new CloseStaleSessionsUseCase(
            presences, new FakeStationVacancyRepository(), new FakeStationUnknownRepository());

        var result = await useCase.ExecuteAsync(MaxOpenAge, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value);
        Assert.Equal(3600, closed.DurationSeconds);
    }

    [Fact]
    public async Task ExecuteAsync_ClosesStaleVacancyAndUnknownToo()
    {
        var vacancies = new FakeStationVacancyRepository();
        var staleVacancy = StationVacancy.Restore(1, "#2 POSTO", "N111", DateTime.Now.AddHours(-72), null, null);
        vacancies.Seed(staleVacancy);

        var unknowns = new FakeStationUnknownRepository();
        var staleUnknown = StationUnknown.Restore(
            1, "#2 POSTO", "N111", DateTime.Now.AddHours(-72), null, null, null, null);
        unknowns.Seed(staleUnknown);

        var useCase = new CloseStaleSessionsUseCase(new FakeStationPresenceRepository(), vacancies, unknowns);

        var result = await useCase.ExecuteAsync(MaxOpenAge, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.False(staleVacancy.IsOpen);
        Assert.False(staleUnknown.IsOpen);
    }
}
