using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.UseCases.Matrix;
using IrisWeb.Application.UseCases.Stations;
using IrisWeb.Application.Validators.Stations;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Rules;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class LineTimelineUseCaseTests
{
    private static readonly DateOnly Yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

    private static DateTime At(int hour, int minute = 0) => Yesterday.ToDateTime(new TimeOnly(hour, minute));

    private readonly FakeStationPresenceRepository _presences = new();
    private readonly FakeStationVacancyRepository _vacancies = new();
    private readonly FakeStationUnknownRepository _unknowns = new();

    private GetLineTimelineUseCase UseCase() => new(_presences, _vacancies, _unknowns);

    private static LineTimelineQuery Query(TimeOnly? from = null, TimeOnly? to = null) =>
        new() { Line = " n111 ", Date = Yesterday, From = from, To = to };

    private void Presence(long id, string workstation, string registration, DateTime start, DateTime end, string? level, string? allocated = null) =>
        _presences.Seed(StationPresence.Restore(id, registration, workstation, "N111", start, end,
            (int)(end - start).TotalSeconds, $"OPERADOR {registration}", level, allocated));

    [Fact]
    public async Task Segments_AreClippedToTheShiftAndClassifiedByTraining()
    {
        Presence(1, "#14 PRENSA", "16271", At(5, 30), At(7), "Qualificado", allocated: "16271");
        Presence(2, "#14 PRENSA", "99999", At(8), At(9), "Sem treinamento", allocated: "16271");
        _vacancies.Seed(StationVacancy.Restore(3, "#14 PRENSA", "N111", At(7), At(8), 3600));

        var result = await UseCase().ExecuteAsync(Query(new TimeOnly(6, 0), new TimeOnly(14, 0)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var station = Assert.Single(result.Value.Stations);
        Assert.Equal(14, station.StationId);
        Assert.Equal("PRENSA", station.Name);
        Assert.Equal(["presence", "vacancy", "presence"], station.Segments.Select(segment => segment.Kind));

        var first = station.Segments[0];
        Assert.Equal(At(6), first.StartedAt);
        Assert.Equal("trained", first.Training);
        Assert.True(first.MatchesAllocation);

        var intruder = station.Segments[2];
        Assert.Equal("untrained", intruder.Training);
        Assert.False(intruder.MatchesAllocation);

        Assert.Equal(3600, station.Totals.TrainedSeconds);
        Assert.Equal(3600, station.Totals.UntrainedSeconds);
        Assert.Equal(3600, station.Totals.VacantSeconds);
        Assert.Equal(3600, station.Totals.MismatchSeconds);
        Assert.Equal(8 * 3600, station.Totals.WindowSeconds);
    }

    [Fact]
    public async Task RootTotals_UseStationTimeAndOrderStationsById()
    {
        Presence(1, "#20 SOLDA", "111", At(6), At(7), "Qualificado");
        Presence(2, "#3 MONTAGEM", "222", At(6), At(8), null);

        var result = await UseCase().ExecuteAsync(Query(new TimeOnly(6, 0), new TimeOnly(10, 0)), CancellationToken.None);

        Assert.Equal([3, 20], result.Value.Stations.Select(station => station.StationId));
        Assert.Equal(2, result.Value.Totals.StationCount);
        Assert.Equal(2 * 4 * 3600, result.Value.Totals.WindowSeconds);
        Assert.Equal(3 * 3600, result.Value.Totals.OccupiedSeconds);
        Assert.Equal(2 * 3600, result.Value.Totals.UnknownLevelSeconds);
        Assert.Equal(At(6), result.Value.FirstActivityAt);
        Assert.Equal(At(8), result.Value.LastActivityAt);
    }

    [Fact]
    public async Task People_SumTimeAcrossStations()
    {
        Presence(1, "#1 A", "0016271", At(6), At(7), "Multiplicador");
        Presence(2, "#2 B", "16271", At(8), At(8, 30), "Multiplicador");

        var result = await UseCase().ExecuteAsync(Query(), CancellationToken.None);

        var person = Assert.Single(result.Value.People);
        Assert.Equal(5400, person.Seconds);
        Assert.Equal("trained", person.Training);
        Assert.Equal(["#1 A", "#2 B"], person.Workstations);
    }

    [Fact]
    public async Task UnknownFaces_KeepTheClosestCandidate()
    {
        _unknowns.Seed(StationUnknown.Restore(1, "#14 PRENSA", "N111", At(9), At(9, 10), 600, "16271", 0.91f));

        var result = await UseCase().ExecuteAsync(Query(), CancellationToken.None);

        var segment = Assert.Single(Assert.Single(result.Value.Stations).Segments);
        Assert.Equal("unknown", segment.Kind);
        Assert.Equal("16271", segment.CandidateRegistration);
        Assert.Equal(600, result.Value.Totals.UnknownFaceSeconds);
    }

    [Fact]
    public async Task OtherLines_AreLeftOut()
    {
        _presences.Seed(StationPresence.Restore(1, "111", "#1 A", "N105", At(6), At(7), 3600));

        var result = await UseCase().ExecuteAsync(Query(), CancellationToken.None);

        Assert.Empty(result.Value.Stations);
        Assert.Null(result.Value.FirstActivityAt);
    }

    [Fact]
    public void Window_CrossingMidnight_EndsTheNextDay()
    {
        var (from, to) = GetLineTimelineUseCase.Window(Yesterday, new TimeOnly(22, 0), new TimeOnly(6, 0));

        Assert.Equal(At(22), from);
        Assert.Equal(At(6).AddDays(1), to);
    }

    [Fact]
    public void Validator_RequiresBothEndsOfTheWindow()
    {
        var validator = new LineTimelineQueryValidator();

        Assert.False(validator.Validate(new LineTimelineQuery { Line = "N111", Date = Yesterday, From = new TimeOnly(6, 0) }).IsValid);
        Assert.False(validator.Validate(new LineTimelineQuery { Line = "N111" }).IsValid);
        Assert.True(validator.Validate(Query()).IsValid);
    }

    [Theory]
    [InlineData("Qualificado", TrainingStatus.Trained)]
    [InlineData("MULTIPLICADOR", TrainingStatus.Trained)]
    [InlineData("Não qualificado", TrainingStatus.Untrained)]
    [InlineData("Sem treinamento", TrainingStatus.Untrained)]
    [InlineData("Não treinado", TrainingStatus.Untrained)]
    [InlineData("Em treinamento", TrainingStatus.Untrained)]
    [InlineData(null, TrainingStatus.Unknown)]
    [InlineData("  ", TrainingStatus.Unknown)]
    public void TrainingLevel_FollowsTheSyncRanks(string? level, TrainingStatus expected) =>
        Assert.Equal(expected, TrainingLevelPolicy.Classify(level));

    [Fact]
    public async Task OpenPresence_AllocatedPerson_TakesNameAndLevelFromTheLayout()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        bindings.Seed(DeviceMatrixBinding.Create("dev1", "N111", "SKU", "1º TURNO", 14, "PRENSA"));
        var sync = new FakeSyncMatrixClient
        {
            Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271, level: "Qualificado")]),
        };
        var presences = new FakeStationPresenceRepository();

        await new OpenDevicePresenceUseCase(bindings, presences, sync)
            .ExecuteAsync("dev1", new OpenDevicePresenceRequest("16271"), CancellationToken.None);

        var presence = Assert.Single(presences.Stored);
        Assert.Equal("FULANO", presence.CollaboratorName);
        Assert.Equal("Qualificado", presence.TrainingLevel);
        Assert.Equal("16271", presence.AllocatedRegistration);
    }

    [Fact]
    public async Task OpenPresence_SomeoneElse_LooksUpTheLevelOnTheStationActivity()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        bindings.Seed(DeviceMatrixBinding.Create("dev1", "N111", "SKU", "1º TURNO", 14, "PRENSA"));
        var sync = new FakeSyncMatrixClient
        {
            Layout = new SyncLayout(35, [FakeSyncMatrixClient.Station(14, "PRENSA", registration: 16271, level: "Qualificado")]),
        };
        sync.ActivityCollaborators[99999] = new SyncCollaboratorInformation(7, "CICLANO", "Sem treinamento", "OPERADOR", 0);
        var presences = new FakeStationPresenceRepository();

        await new OpenDevicePresenceUseCase(bindings, presences, sync)
            .ExecuteAsync("dev1", new OpenDevicePresenceRequest("99999"), CancellationToken.None);

        var presence = Assert.Single(presences.Stored);
        Assert.Equal("CICLANO", presence.CollaboratorName);
        Assert.Equal("Sem treinamento", presence.TrainingLevel);
        Assert.Equal("16271", presence.AllocatedRegistration);
        Assert.Equal((99999, 429, "SKU", "1º TURNO", 14, 35), sync.LastCandidateQuery);
    }

    [Fact]
    public async Task OpenPresence_SyncDown_StillOpensWithoutSnapshot()
    {
        var bindings = new FakeDeviceMatrixBindingRepository();
        bindings.Seed(DeviceMatrixBinding.Create("dev1", "N111", "SKU", "1º TURNO", 14, "PRENSA"));
        var sync = new FakeSyncMatrixClient { Failure = new HttpRequestException("sem rede") };
        var presences = new FakeStationPresenceRepository();

        var result = await new OpenDevicePresenceUseCase(bindings, presences, sync)
            .ExecuteAsync("dev1", new OpenDevicePresenceRequest("16271"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var presence = Assert.Single(presences.Stored);
        Assert.Null(presence.TrainingLevel);
        Assert.Null(presence.AllocatedRegistration);
    }
}
