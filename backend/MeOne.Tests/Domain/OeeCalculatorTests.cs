using MeOne.Application.Communication.Production.Requests;
using MeOne.Domain.Entities;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Domain;

public sealed class OeeCalculatorTests
{
    private static async Task<(ProductionFixture Fixture, ProductionRun Run)> RunningAsync()
    {
        var fixture = new ProductionFixture();
        var run = await fixture.StartedRunAsync();
        return (fixture, run);
    }

    private static async Task Record(ProductionFixture f, ProductionRun run, string post, int quantity)
    {
        var result = await f.RecordOutput.ExecuteAsync(
            run.Id,
            new RecordOutputRequest(f.PostId(run, post), quantity, run.StartedAt),
            CancellationToken.None);

        Assert.True(result.IsSuccess, string.Join(";", result.Errors.Select(error => error.Message)));
    }

    private static PostOee Post(RunOee oee, string name) => oee.Posts.Single(post => post.Name == name);

    [Fact]
    public async Task RunThatDidNotStart_HasNoOee()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();

        var oee = OeeCalculator.Default.Calculate(run, DateTime.UtcNow, OeeLosses.None);

        Assert.False(oee.Started);
        Assert.Equal(4, oee.Posts.Count);
        Assert.All(oee.Posts, post => Assert.False(post.Measured));
        Assert.Equal(0, oee.Line.Pillars.Oee);
    }

    [Fact]
    public async Task Post_WithDefaultFormulas_UsesProducedAgainstTheTheoretical()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Montagem", 80);

        var oee = OeeCalculator.Default.Calculate(run, run.StartedAt!.Value.AddHours(1), OeeLosses.None);
        var montagem = Post(oee, "Montagem");

        Assert.True(montagem.Measured);
        Assert.Equal(1, montagem.Pillars.Availability, 9);
        Assert.Equal(0.8, montagem.Pillars.Performance, 9);
        Assert.Equal(1, montagem.Pillars.Quality, 9);
        Assert.Equal(0.8, montagem.Pillars.Oee, 9);
        Assert.Equal(100, montagem.Variables["theoretical_production"], 6);
        Assert.Equal(80, montagem.Variables["real_production"], 9);
        Assert.False(oee.LossSourceAvailable);
    }

    [Fact]
    public async Task Post_WithoutOutputs_HasZeroPerformanceAndOee()
    {
        var (_, run) = await RunningAsync();

        var oee = OeeCalculator.Default.Calculate(run, run.StartedAt!.Value.AddHours(2), OeeLosses.None);

        Assert.All(oee.Posts, post => Assert.Equal(0, post.Pillars.Performance));
        Assert.All(oee.Posts, post => Assert.Equal(0, post.Pillars.Oee));
    }

    [Fact]
    public async Task Downtime_ReducesAvailabilityAndDefectsReduceQuality()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Montagem", 80);
        var started = run.StartedAt!.Value;
        var montagem = f.PostId(run, "Montagem");

        var losses = new OeeLosses(
            [new OeeDowntimeEvent(montagem, started.AddMinutes(10), started.AddMinutes(20), true, false, OeeDowntimeSource.Ticket)],
            [new OeeDefectEvent(montagem, started.AddMinutes(30), 8, false)],
            Available: true);

        var oee = OeeCalculator.Default.Calculate(run, started.AddHours(1), losses);
        var post = Post(oee, "Montagem");

        Assert.True(oee.LossSourceAvailable);
        Assert.Equal(3000.0 / 3600, post.Pillars.Availability, 9);
        Assert.Equal(72.0 / 80, post.Pillars.Quality, 9);
        Assert.Equal(600, post.Variables["downtime_availability"], 6);
        Assert.Equal(600, post.Variables["ticket_downtime_proportional"], 6);
        Assert.Equal(8, post.Variables["defects"]);
        Assert.Equal(1, Post(oee, "Maquina A").Pillars.Availability, 9);
    }

    [Fact]
    public async Task Defects_WithoutAPost_AreChargedToTheLastMeasuredPostOnly()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Qualidade", 100);
        await Record(f, run, "Montagem", 100);
        var started = run.StartedAt!.Value;

        var losses = new OeeLosses([], [new OeeDefectEvent(null, started.AddMinutes(5), 10, false)], Available: true);

        var oee = OeeCalculator.Default.Calculate(run, started.AddHours(1), losses);

        Assert.Equal(10, Post(oee, "Qualidade").Variables["defects"]);
        Assert.Equal(0, Post(oee, "Montagem").Variables["defects"]);
        Assert.Equal(0.9, Post(oee, "Qualidade").Pillars.Quality, 9);
        Assert.Equal(1, Post(oee, "Montagem").Pillars.Quality, 9);
    }

    [Fact]
    public async Task Downtime_WithoutAPost_HitsEveryPost()
    {
        var (_, run) = await RunningAsync();
        var started = run.StartedAt!.Value;

        var losses = new OeeLosses(
            [new OeeDowntimeEvent(null, started, started.AddMinutes(30), true, false, OeeDowntimeSource.Real)],
            [],
            Available: true);

        var oee = OeeCalculator.Default.Calculate(run, started.AddHours(1), losses);

        Assert.All(oee.Posts, post => Assert.Equal(0.5, post.Pillars.Availability, 9));
    }

    [Fact]
    public async Task Line_UsesAverageAvailabilityLastPostPerformanceAndProductQuality()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Montagem", 100);
        await Record(f, run, "Qualidade", 60);
        var started = run.StartedAt!.Value;
        var qualidade = f.PostId(run, "Qualidade");

        var losses = new OeeLosses(
            [new OeeDowntimeEvent(qualidade, started, started.AddMinutes(36), true, false, OeeDowntimeSource.Real)],
            [new OeeDefectEvent(qualidade, started.AddMinutes(5), 6, false)],
            Available: true);

        var oee = OeeCalculator.Default.Calculate(run, started.AddHours(1), losses);

        Assert.Equal(qualidade, oee.Line.LastPostId);
        Assert.Equal(0.85, oee.Line.Pillars.Availability, 9);
        Assert.Equal(0.5, oee.Line.Pillars.Performance, 9);
        Assert.Equal(0.9, oee.Line.Pillars.Quality, 9);
        Assert.Equal(160, oee.Line.Variables["line_produced"], 9);
        Assert.Equal(60, oee.Line.Variables["line_produced_station"], 9);
        Assert.Equal(120, oee.Line.Variables["line_plan"], 6);
        Assert.Equal(oee.Line.Pillars.Availability * oee.Line.Pillars.Performance * oee.Line.Pillars.Quality, oee.Line.Pillars.Oee, 9);
    }

    [Fact]
    public async Task CustomFormulas_ChangeTheResult()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Montagem", 80);

        var formulas = OeeFormulas.Default with
        {
            Performance = "min(real_production / theoretical_production, 0.5)",
            MachineOee = "availability * performance",
            LineOee = "performance_last_post + 0.25",
        };

        var oee = new OeeCalculator(formulas).Calculate(run, run.StartedAt!.Value.AddHours(1), OeeLosses.None);

        Assert.Equal(0.5, Post(oee, "Montagem").Pillars.Performance, 9);
        Assert.Equal(0.5, Post(oee, "Montagem").Pillars.Oee, 9);
        Assert.Equal(0.25, oee.Line.Pillars.Oee, 9);
    }

    [Fact]
    public async Task BrokenFormula_FallsBackToTheDefault()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Montagem", 80);

        var oee = new OeeCalculator(OeeFormulas.Default with { Performance = "((" }).Calculate(run, run.StartedAt!.Value.AddHours(1), OeeLosses.None);

        Assert.Equal(0.8, Post(oee, "Montagem").Pillars.Performance, 9);
    }

    [Fact]
    public async Task Hours_SplitTheRunAndSumToTheWholeProduction()
    {
        var (f, run) = await RunningAsync();
        await Record(f, run, "Montagem", 80);

        var oee = OeeCalculator.Default.Calculate(run, run.StartedAt!.Value.AddHours(2.5), OeeLosses.None);
        var montagem = Post(oee, "Montagem");

        Assert.True(montagem.Hours.Count >= 3);
        Assert.Equal(80, montagem.Hours.Sum(hour => hour.Produced), 9);
        Assert.Equal(montagem.Hours.Count, oee.Line.Hours.Count);
        Assert.Equal(80, oee.Line.Hours.Sum(hour => hour.Produced), 9);
    }

    [Fact]
    public async Task PostsWithoutTarget_AreNotMeasured()
    {
        var f = new ProductionFixture();
        var layout = await f.LayoutAsync(ProductionFixture.Layout(
        [
            ProductionFixture.Post("Montagem", target: 100),
            ProductionFixture.Post("Apoio", x: 300),
        ]));
        var run = await f.RunAsync(layout);
        await f.AllocateAsync(run, "Montagem", f.Ana);
        await f.Start.ExecuteAsync(run.Id, CancellationToken.None);

        var oee = OeeCalculator.Default.Calculate(run, run.StartedAt!.Value.AddHours(1), OeeLosses.None);

        Assert.False(Post(oee, "Apoio").Measured);
        Assert.Equal(0, Post(oee, "Apoio").Pillars.Oee);
        Assert.Empty(Post(oee, "Apoio").Hours);
    }

    [Fact]
    public async Task ClosedRun_StopsTheClockAtTheEnd()
    {
        var (f, run) = await RunningAsync();
        await f.Close.ExecuteAsync(run.Id, CancellationToken.None);

        var oee = OeeCalculator.Default.Calculate(run, run.EndedAt!.Value.AddHours(10), OeeLosses.None);

        Assert.Equal(run.EndedAt, oee.To);
        Assert.True(oee.ElapsedSeconds < 60);
    }
}
