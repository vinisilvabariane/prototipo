using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.UseCases.Argos;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class ArgosUseCaseTests
{
    private static readonly DateTime Day = new(2026, 10, 1);

    private static ArgosCycle Cycle(
        long id,
        DateTime at,
        bool ok,
        decimal seconds = 10m,
        string? station = "RV-01",
        string? setup = "CABO-A",
        string? reason = null,
        string? point = null) =>
        ArgosCycle.Restore(id, ok ? 1 : 0, seconds, at, station, setup, reason, point);

    private static FakeArgosCycleRepository Seeded()
    {
        var repository = new FakeArgosCycleRepository();
        repository.Seed(
            Cycle(1, Day.AddHours(8), true, 10m),
            Cycle(2, Day.AddHours(8).AddMinutes(30), false, 12m, reason: "CABLE_ABSENT", point: "P3"),
            Cycle(3, Day.AddHours(9), true, 11m, station: "RV-02"),
            Cycle(4, Day.AddHours(18), false, 14m, station: "RV-02", reason: "RF_DETECTED"),
            Cycle(5, Day.AddHours(18).AddMinutes(10), true, 9m, station: "RV-02"),
            Cycle(6, Day.AddHours(5), true, 10m));
        return repository;
    }

    private static ArgosSummaryQuery Query(Action<ArgosSummaryQueryBuilder>? tweak = null)
    {
        var builder = new ArgosSummaryQueryBuilder();
        tweak?.Invoke(builder);
        return builder.Build();
    }

    private sealed class ArgosSummaryQueryBuilder
    {
        public ArgosShiftFilter Shift { get; set; } = ArgosShiftFilter.All;
        public ArgosResultFilter Result { get; set; } = ArgosResultFilter.All;
        public ArgosBucket Bucket { get; set; } = ArgosBucket.Hour;
        public string? Station { get; set; }

        public ArgosSummaryQuery Build() => new()
        {
            From = Day,
            To = Day.AddDays(1),
            Shift = Shift,
            Result = Result,
            Bucket = Bucket,
            Station = Station,
        };
    }

    [Fact]
    public async Task Summary_CountsTotalsAndAveragesTheWholeWindow()
    {
        var result = await new GetArgosSummaryUseCase(Seeded()).ExecuteAsync(Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value.Totals.Ok);
        Assert.Equal(2, result.Value.Totals.Ng);
        Assert.Equal(11.0, result.Value.Totals.AvgCycleSeconds);
        Assert.Equal("hour", result.Value.Bucket);
    }

    [Fact]
    public async Task Summary_ByShiftIgnoresTheShiftFilterButKeepsTheRest()
    {
        var onlyAdm = await new GetArgosSummaryUseCase(Seeded())
            .ExecuteAsync(Query(q => q.Shift = ArgosShiftFilter.Adm), CancellationToken.None);

        Assert.Equal(3, onlyAdm.Value.Totals.Ok + onlyAdm.Value.Totals.Ng);
        Assert.Equal(2, onlyAdm.Value.ByShift.Adm.Ok);
        Assert.Equal(1, onlyAdm.Value.ByShift.Adm.Ng);
        Assert.Equal(1, onlyAdm.Value.ByShift.T2.Ok);
        Assert.Equal(1, onlyAdm.Value.ByShift.T2.Ng);
    }

    [Fact]
    public async Task Summary_ShiftGapBetweenT2AndAdmBelongsToNoShift()
    {
        var all = await new GetArgosSummaryUseCase(Seeded()).ExecuteAsync(Query(), CancellationToken.None);

        var counted = all.Value.ByShift.Adm.Ok + all.Value.ByShift.Adm.Ng
            + all.Value.ByShift.T2.Ok + all.Value.ByShift.T2.Ng;

        Assert.Equal(5, counted);
        Assert.Equal(6, all.Value.Totals.Ok + all.Value.Totals.Ng);
    }

    [Fact]
    public async Task Summary_BucketsOnlyHaveHoursWithPiecesAndSplitByStart()
    {
        var result = await new GetArgosSummaryUseCase(Seeded()).ExecuteAsync(Query(), CancellationToken.None);

        Assert.Equal(
            [Day.AddHours(5), Day.AddHours(8), Day.AddHours(9), Day.AddHours(18)],
            result.Value.Buckets.Select(bucket => bucket.Start));
        Assert.Equal(1, result.Value.Buckets[1].Ok);
        Assert.Equal(1, result.Value.Buckets[1].Ng);
    }

    [Fact]
    public async Task Summary_DayBucketGroupsTheWholeDay()
    {
        var result = await new GetArgosSummaryUseCase(Seeded())
            .ExecuteAsync(Query(q => q.Bucket = ArgosBucket.Day), CancellationToken.None);

        var bucket = Assert.Single(result.Value.Buckets);
        Assert.Equal(Day, bucket.Start);
        Assert.Equal(6, bucket.Ok + bucket.Ng);
    }

    [Fact]
    public async Task Summary_FailReasonsAndPointsCountOnlyNg()
    {
        var result = await new GetArgosSummaryUseCase(Seeded()).ExecuteAsync(Query(), CancellationToken.None);

        Assert.Equal(["CABLE_ABSENT", "RF_DETECTED"], result.Value.FailReasons.Select(item => item.Reason));
        var point = Assert.Single(result.Value.FailedPoints);
        Assert.Equal("P3", point.Point);
        Assert.Equal(1, point.Count);
    }

    [Fact]
    public async Task Summary_StationsAreGroupedWithTheirLastPiece()
    {
        var result = await new GetArgosSummaryUseCase(Seeded()).ExecuteAsync(Query(), CancellationToken.None);

        Assert.Equal(["RV-01", "RV-02"], result.Value.Stations.Select(item => item.Station));
        Assert.Equal(Day.AddHours(18).AddMinutes(10), result.Value.Stations[1].LastCycleAt);
    }

    [Fact]
    public async Task Summary_LastCycleIgnoresWindowShiftAndResult()
    {
        var repository = Seeded();
        repository.Seed(Cycle(7, Day.AddDays(3), false));

        var result = await new GetArgosSummaryUseCase(repository)
            .ExecuteAsync(Query(q => q.Result = ArgosResultFilter.Ok), CancellationToken.None);

        Assert.Equal(Day.AddDays(3), result.Value.LastCycleAt);
    }

    [Fact]
    public async Task Summary_NoPieces_HasNullAverageAndNullLastCycle()
    {
        var result = await new GetArgosSummaryUseCase(new FakeArgosCycleRepository())
            .ExecuteAsync(Query(), CancellationToken.None);

        Assert.Equal(0, result.Value.Totals.Ok);
        Assert.Null(result.Value.Totals.AvgCycleSeconds);
        Assert.Null(result.Value.LastCycleAt);
        Assert.Empty(result.Value.Buckets);
    }

    [Fact]
    public async Task Cycles_ListsNewestFirstWithTextStatus()
    {
        var result = await new ListArgosCyclesUseCase(Seeded()).ExecuteAsync(
            new ArgosCyclesQuery { From = Day, To = Day.AddDays(1), PageSize = 2 },
            CancellationToken.None);

        Assert.Equal(6, result.Value.Total);
        Assert.Equal([5L, 4L], result.Value.Entries.Select(entry => entry.Id));
        Assert.Equal("ok", result.Value.Entries[0].Status);
        Assert.Equal("ng", result.Value.Entries[1].Status);
    }

    [Fact]
    public async Task Cycles_PageBeyondTheEndReturnsTheLastPage()
    {
        var result = await new ListArgosCyclesUseCase(Seeded()).ExecuteAsync(
            new ArgosCyclesQuery { From = Day, To = Day.AddDays(1), PageSize = 4, Page = 9 },
            CancellationToken.None);

        Assert.Equal(2, result.Value.Page);
        Assert.Equal(2, result.Value.Entries.Count);
        Assert.Equal(6, result.Value.Total);
    }

    [Fact]
    public async Task Cycles_DefaultPageSizeIs50AndIsCappedAt500()
    {
        var repository = Seeded();

        var defaults = await new ListArgosCyclesUseCase(repository).ExecuteAsync(
            new ArgosCyclesQuery { From = Day, To = Day.AddDays(1) }, CancellationToken.None);
        var capped = await new ListArgosCyclesUseCase(repository).ExecuteAsync(
            new ArgosCyclesQuery { From = Day, To = Day.AddDays(1), PageSize = 9_999 }, CancellationToken.None);

        Assert.Equal(50, defaults.Value.PageSize);
        Assert.Equal(500, capped.Value.PageSize);
    }

    [Fact]
    public async Task Cycles_FiltersByResultAndStation()
    {
        var result = await new ListArgosCyclesUseCase(Seeded()).ExecuteAsync(
            new ArgosCyclesQuery
            {
                From = Day,
                To = Day.AddDays(1),
                Result = ArgosResultFilter.Ng,
                Station = " RV-02 ",
            },
            CancellationToken.None);

        var entry = Assert.Single(result.Value.Entries);
        Assert.Equal(4, entry.Id);
    }

    [Fact]
    public async Task Filters_ReturnDistinctSortedStationsAndSetupsOfTheLastDays()
    {
        var repository = new FakeArgosCycleRepository();
        repository.Seed(Cycle(8, DateTime.Now.AddDays(-200), true, station: "RV-ANTIGA", setup: "VELHO"));
        repository.Seed(Cycle(9, DateTime.Now.AddMinutes(-5), true, station: "RV-00", setup: "CABO-B"));

        var result = await new ListArgosFiltersUseCase(repository).ExecuteAsync(CancellationToken.None);

        Assert.Equal(["RV-00"], result.Value.Stations);
        Assert.Equal(["CABO-B"], result.Value.Setups);
    }
}
