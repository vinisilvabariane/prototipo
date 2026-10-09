using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Application.UseCases.Stations;
using IrisWeb.Domain.Entities;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class ExportStationRecordsUseCaseTests
{
    private sealed class CapturingWriter : IStationExportWriter
    {
        public IReadOnlyList<StationExportRow> Rows { get; private set; } = [];

        public bool Truncated { get; private set; }

        public Stream Write(IReadOnlyList<StationExportRow> rows, DateTime from, DateTime to, bool truncated)
        {
            Rows = rows;
            Truncated = truncated;
            return new MemoryStream();
        }
    }

    private static readonly DateTime Day = new(2026, 9, 25, 8, 0, 0);

    [Fact]
    public async Task ExecuteAsync_JoinsTheThreeTablesInChronologicalOrder()
    {
        var presences = new FakeStationPresenceRepository();
        presences.Seed(StationPresence.Restore(
            1, "15834", "#1 INSPEÇÃO", "N111", Day.AddMinutes(10), Day.AddMinutes(20), 600,
            "FULANO", "Treinado", "15834"));

        var vacancies = new FakeStationVacancyRepository();
        vacancies.Seed(StationVacancy.Restore(1, "#1 INSPEÇÃO", "N111", Day, Day.AddMinutes(10), 600));

        var unknowns = new FakeStationUnknownRepository();
        unknowns.Seed(StationUnknown.Restore(
            1, "#1 INSPEÇÃO", "N111", Day.AddMinutes(30), Day.AddMinutes(31), 60, "99999", 0.9f));

        var writer = new CapturingWriter();
        var useCase = new ExportStationRecordsUseCase(presences, vacancies, unknowns, writer);

        var result = await useCase.ExecuteAsync(
            new StationTimelineQuery { From = Day.Date, To = Day.Date.AddDays(1), Line = "N111" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["vacancy", "presence", "unknown"], writer.Rows.Select(row => row.Kind));
        Assert.Equal("FULANO", writer.Rows[1].CollaboratorName);
        Assert.Equal("Treinado", writer.Rows[1].TrainingLevel);
        Assert.Equal("99999", writer.Rows[2].CandidateRegistration);
        Assert.False(writer.Truncated);
        Assert.Equal("registros-N111-20260925-20260926.xlsx", result.Value.FileName);
    }

    [Fact]
    public async Task ExecuteAsync_FilteringByRegistration_LeavesVacancyAndUnknownOut()
    {
        var presences = new FakeStationPresenceRepository();
        presences.Seed(StationPresence.Restore(
            1, "15834", "#1 POSTO", "N111", Day, Day.AddMinutes(5), 300));

        var vacancies = new FakeStationVacancyRepository();
        vacancies.Seed(StationVacancy.Restore(1, "#1 POSTO", "N111", Day.AddMinutes(5), Day.AddMinutes(9), 240));

        var writer = new CapturingWriter();
        var useCase = new ExportStationRecordsUseCase(
            presences, vacancies, new FakeStationUnknownRepository(), writer);

        await useCase.ExecuteAsync(
            new StationTimelineQuery { From = Day.Date, To = Day.Date.AddDays(1), Registration = "15834" },
            CancellationToken.None);

        var row = Assert.Single(writer.Rows);
        Assert.Equal("presence", row.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_OpenSessionCountsUntilTheEndOfTheWindowOrNow()
    {
        var presences = new FakeStationPresenceRepository();
        presences.Seed(StationPresence.Restore(1, "15834", "#1 POSTO", "N111", Day, null, null));

        var writer = new CapturingWriter();
        var useCase = new ExportStationRecordsUseCase(
            presences, new FakeStationVacancyRepository(), new FakeStationUnknownRepository(), writer);

        await useCase.ExecuteAsync(
            new StationTimelineQuery { From = Day.Date, To = Day.Date.AddDays(1) },
            CancellationToken.None);

        var row = Assert.Single(writer.Rows);
        Assert.Null(row.EndedAt);
        Assert.True(row.DurationSeconds > 0);
    }
}
