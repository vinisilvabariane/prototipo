using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Validators.Oee;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class OeeUseCaseTests
{
    private static string Message(FluentResults.ResultBase result) => Assert.Single(result.Errors).Message;

    private static OeeFormulasRequest Formulas(string? performance = null) => new(
        OeeFormulas.Default.Availability,
        performance ?? OeeFormulas.Default.Performance,
        OeeFormulas.Default.Quality,
        OeeFormulas.Default.MachineOee,
        OeeFormulas.Default.LineAvailability,
        OeeFormulas.Default.LinePerformance,
        OeeFormulas.Default.LineQuality,
        OeeFormulas.Default.LineOee);

    private static SaveOeeFormulaSetRequest Save(string name, string? performance = null, bool activate = false) =>
        new(name, Formulas(performance), activate);

    [Fact]
    public async Task RunOee_ForAnUnknownRun_IsNotFound()
    {
        var f = new ProductionFixture();

        var result = await f.GetRunOee.ExecuteAsync(999, CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task RunOee_ForARunInPreparation_HasNoNumbersYet()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();

        var result = await f.GetRunOee.ExecuteAsync(run.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.StartedAt);
        Assert.Equal(0, result.Value.Line.Oee);
        Assert.Equal(4, result.Value.Posts.Count);
        Assert.Equal(OeeMapping.Default, result.Value.FormulaSetName);
    }

    [Fact]
    public async Task RunOee_ReportsPercentagesAndTheFormulaSetInUse()
    {
        var f = new ProductionFixture();
        var run = await f.StartedRunAsync();
        await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(f.PostId(run, "Montagem"), 80, null), CancellationToken.None);

        var created = await f.CreateFormulaSet.ExecuteAsync(Save("Meta fixa", "real_production / 160", activate: true), CancellationToken.None);
        var result = await f.GetRunOee.ExecuteAsync(run.Id, CancellationToken.None);

        Assert.True(created.IsSuccess);
        Assert.Equal("Meta fixa", result.Value.FormulaSetName);
        var montagem = result.Value.Posts.Single(post => post.Name == "Montagem");
        Assert.Equal(50, montagem.Pillars.Performance);
        Assert.Equal(80, montagem.Produced);
        Assert.False(result.Value.LossSourceAvailable);
    }

    [Fact]
    public async Task RunOee_UsesTheLossSource()
    {
        var f = new ProductionFixture();
        var run = await f.StartedRunAsync();
        var started = run.StartedAt!.Value;
        f.LossSource.Losses = new OeeLosses(
            [new OeeDowntimeEvent(null, started, started.AddSeconds(1), true, false, OeeDowntimeSource.Real)],
            [],
            Available: true);

        var result = await f.GetRunOee.ExecuteAsync(run.Id, CancellationToken.None);

        Assert.True(result.Value.LossSourceAvailable);
    }

    [Fact]
    public async Task ListRuns_ReturnsStartedRunsInThePeriodAndValidatesIt()
    {
        var f = new ProductionFixture();
        var started = await f.StartedRunAsync();
        var prepared = await f.RunAsync(await f.LayoutAsync(line: await NewLine(f)));

        var all = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery(), CancellationToken.None);
        var otherLine = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { LineId = prepared.LineId }, CancellationToken.None);
        var inverted = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { From = DateTime.UtcNow, To = DateTime.UtcNow.AddDays(-1) }, CancellationToken.None);
        var tooLong = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { From = DateTime.UtcNow.AddDays(-200), To = DateTime.UtcNow }, CancellationToken.None);
        var past = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { From = DateTime.UtcNow.AddDays(-30), To = DateTime.UtcNow.AddDays(-20) }, CancellationToken.None);

        Assert.Equal([started.Id], all.Value.Runs.Select(run => run.RunId));
        Assert.Equal(4, all.Value.Runs[0].MeasuredPosts);
        Assert.Empty(otherLine.Value.Runs);
        Assert.Equal(OeeErrors.PeriodInvalid, Message(inverted));
        Assert.Equal(OeeErrors.PeriodTooLong, Message(tooLong));
        Assert.Empty(past.Value.Runs);
    }

    [Fact]
    public async Task ListRuns_FiltersByStatus()
    {
        var f = new ProductionFixture();
        var run = await f.StartedRunAsync();

        var running = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { Status = MeOne.Domain.Enums.RunStatus.Running }, CancellationToken.None);
        await f.Close.ExecuteAsync(run.Id, CancellationToken.None);
        var stillRunning = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { Status = MeOne.Domain.Enums.RunStatus.Running }, CancellationToken.None);
        var closed = await f.ListOeeRuns.ExecuteAsync(new OeeRunsQuery { Status = MeOne.Domain.Enums.RunStatus.Closed }, CancellationToken.None);

        Assert.Single(running.Value.Runs);
        Assert.Empty(stillRunning.Value.Runs);
        Assert.Single(closed.Value.Runs);
    }

    private static async Task<MeOne.Domain.Entities.ProductionLine> NewLine(ProductionFixture f)
    {
        var line = MeOne.Domain.Entities.ProductionLine.Create("Linha 2", DateTime.UtcNow);
        f.Lines.Insert(line);
        return line;
    }

    [Fact]
    public async Task FormulaSet_IsCreatedUpdatedActivatedAndDeleted()
    {
        var f = new ProductionFixture();

        var first = await f.CreateFormulaSet.ExecuteAsync(Save(" Padrão da casa ", activate: true), CancellationToken.None);
        var second = await f.CreateFormulaSet.ExecuteAsync(Save("Alternativo"), CancellationToken.None);
        var duplicate = await f.CreateFormulaSet.ExecuteAsync(Save("padrão da casa"), CancellationToken.None);

        Assert.Equal("Padrão da casa", first.Value.Name);
        Assert.True(first.Value.IsActive);
        Assert.False(second.Value.IsActive);
        Assert.Equal("Sonia Supervisora", first.Value.UpdatedByName);
        Assert.Equal(OeeErrors.NameTaken("padrão da casa"), Message(duplicate));

        var renamed = await f.UpdateFormulaSet.ExecuteAsync(second.Value.Id, Save("Alternativo 2", "real_production / 200", activate: true), CancellationToken.None);
        var stored = f.FormulaSets.Stored.Single(set => set.Id == second.Value.Id);

        Assert.True(renamed.IsSuccess);
        Assert.Equal("real_production / 200", stored.Performance);
        Assert.True(stored.IsActive);
        Assert.Single(f.FormulaSets.Stored, set => set.IsActive);

        var blocked = await f.DeleteFormulaSet.ExecuteAsync(second.Value.Id, CancellationToken.None);
        Assert.Equal(OeeErrors.ActiveSetCannotBeDeleted, Message(blocked));

        var back = await f.SetActiveFormulaSet.ExecuteAsync(new SetActiveOeeFormulaSetRequest(null), CancellationToken.None);
        var deleted = await f.DeleteFormulaSet.ExecuteAsync(second.Value.Id, CancellationToken.None);
        var missing = await f.DeleteFormulaSet.ExecuteAsync(second.Value.Id, CancellationToken.None);

        Assert.True(back.IsSuccess);
        Assert.DoesNotContain(f.FormulaSets.Stored, set => set.IsActive);
        Assert.True(deleted.IsSuccess);
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
    }

    [Fact]
    public async Task FormulaSet_UpdateRejectsClashingNamesAndMissingSets()
    {
        var f = new ProductionFixture();
        var one = await f.CreateFormulaSet.ExecuteAsync(Save("Um"), CancellationToken.None);
        await f.CreateFormulaSet.ExecuteAsync(Save("Dois"), CancellationToken.None);

        var clash = await f.UpdateFormulaSet.ExecuteAsync(one.Value.Id, Save("dois"), CancellationToken.None);
        var missing = await f.UpdateFormulaSet.ExecuteAsync(999, Save("Tres"), CancellationToken.None);
        var same = await f.UpdateFormulaSet.ExecuteAsync(one.Value.Id, Save("Um"), CancellationToken.None);

        Assert.Equal(OeeErrors.NameTaken("dois"), Message(clash));
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
        Assert.True(same.IsSuccess);
    }

    [Fact]
    public async Task SetActive_RejectsAnUnknownSet()
    {
        var f = new ProductionFixture();

        var result = await f.SetActiveFormulaSet.ExecuteAsync(new SetActiveOeeFormulaSetRequest(77), CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task Config_ListsSetsCatalogAndPermission()
    {
        var f = new ProductionFixture();
        await f.CreateFormulaSet.ExecuteAsync(Save("Meu", activate: true), CancellationToken.None);

        var manager = await f.GetOeeConfig(f.FullAccessActor).ExecuteAsync(CancellationToken.None);
        var reader = await f.GetOeeConfig(FakeCurrentActor.WithPermissions(f.Supervisor.Id, Permissions.ProductionRead)).ExecuteAsync(CancellationToken.None);

        Assert.True(manager.Value.CanManage);
        Assert.False(reader.Value.CanManage);
        Assert.Single(manager.Value.Sets);
        Assert.Equal(manager.Value.Sets[0].Id, manager.Value.ActiveSetId);
        Assert.Equal(8, manager.Value.Slots.Count);
        Assert.Contains(manager.Value.Variables, variable => variable.Key == "real_production" && variable.Slots.Contains("availability"));
        Assert.DoesNotContain(manager.Value.Variables.Single(variable => variable.Key == "line_plan").Slots, slot => slot == "availability");
        Assert.Contains(manager.Value.Functions, function => function.Name == "if");
        Assert.Equal(OeeFormulas.Default.Quality, manager.Value.Default.Quality);
    }

    [Fact]
    public async Task TestFormula_EvaluatesWithManualValues()
    {
        var f = new ProductionFixture();

        var result = await f.TestFormula.ExecuteAsync(
            new TestOeeFormulaRequest("performance", "real_production / theoretical_production", null, null,
                new Dictionary<string, double> { ["real_production"] = 90, ["theoretical_production"] = 120 }),
            CancellationToken.None);

        Assert.True(result.Value.Valid);
        Assert.Equal(0.75, result.Value.Result);
        Assert.Equal(["real_production", "theoretical_production"], result.Value.Identifiers);
    }

    [Fact]
    public async Task TestFormula_ReportsSyntaxAndScopeProblemsWithoutFailing()
    {
        var f = new ProductionFixture();

        var syntax = await f.TestFormula.ExecuteAsync(new TestOeeFormulaRequest("quality", "(1 +", null, null, null), CancellationToken.None);
        var scope = await f.TestFormula.ExecuteAsync(new TestOeeFormulaRequest("quality", "line_plan / 2", null, null, null), CancellationToken.None);

        Assert.True(syntax.IsSuccess);
        Assert.False(syntax.Value.Valid);
        Assert.NotNull(syntax.Value.Error);
        Assert.False(scope.Value.Valid);
        Assert.Contains("line_plan", scope.Value.Error);
    }

    [Fact]
    public async Task TestFormula_CanUseTheRealNumbersOfARun()
    {
        var f = new ProductionFixture();
        var run = await f.StartedRunAsync();
        await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(f.PostId(run, "Montagem"), 80, null), CancellationToken.None);

        var post = await f.TestFormula.ExecuteAsync(
            new TestOeeFormulaRequest("quality", "real_production + 1", run.Id, f.PostId(run, "Montagem"), null),
            CancellationToken.None);
        var line = await f.TestFormula.ExecuteAsync(
            new TestOeeFormulaRequest("lineOee", "line_produced", run.Id, null, null),
            CancellationToken.None);

        Assert.Equal(81, post.Value.Result);
        Assert.Equal(80, line.Value.Result);
    }

    [Fact]
    public async Task TestFormula_NeedsAStartedRun()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();

        var result = await f.TestFormula.ExecuteAsync(new TestOeeFormulaRequest("quality", "1", run.Id, null, null), CancellationToken.None);
        var missing = await f.TestFormula.ExecuteAsync(new TestOeeFormulaRequest("quality", "1", 999, null, null), CancellationToken.None);

        Assert.Equal(OeeErrors.RunNotStarted, Message(result));
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
    }

    [Fact]
    public void SaveValidator_ChecksNameAndEveryFormula()
    {
        var validator = new SaveOeeFormulaSetRequestValidator();

        Assert.True(validator.Validate(Save("Ok")).IsValid);
        Assert.False(validator.Validate(Save(" ")).IsValid);
        Assert.False(validator.Validate(Save(new string('x', OeeErrors.MaxSetNameLength + 1))).IsValid);
        Assert.False(validator.Validate(new SaveOeeFormulaSetRequest("Ok", null, false)).IsValid);

        var syntax = validator.Validate(Save("Ok", "real_production /"));
        var scope = validator.Validate(Save("Ok", "line_plan"));
        var empty = validator.Validate(new SaveOeeFormulaSetRequest("Ok", Formulas() with { Quality = " " }, false));

        Assert.Contains("desempenho do posto", Assert.Single(syntax.Errors).ErrorMessage);
        Assert.Contains("line_plan", Assert.Single(scope.Errors).ErrorMessage);
        Assert.Contains("qualidade do posto", Assert.Single(empty.Errors).ErrorMessage);
    }

    [Fact]
    public void TestValidator_NeedsAKnownSlot()
    {
        var validator = new TestOeeFormulaRequestValidator();

        Assert.True(validator.Validate(new TestOeeFormulaRequest("lineOee", "1", null, null, null)).IsValid);
        Assert.False(validator.Validate(new TestOeeFormulaRequest("other", "1", null, null, null)).IsValid);
        Assert.False(validator.Validate(new TestOeeFormulaRequest(null, "1", null, null, null)).IsValid);
        Assert.False(validator.Validate(new TestOeeFormulaRequest("lineOee", new string('1', OeeErrors.MaxFormulaLength + 1), null, null, null)).IsValid);
    }

    [Fact]
    public void RunsQueryValidator_RejectsInvertedPeriods()
    {
        var validator = new OeeRunsQueryValidator();

        Assert.True(validator.Validate(new OeeRunsQuery()).IsValid);
        Assert.True(validator.Validate(new OeeRunsQuery { From = new(2026, 1, 1), To = new(2026, 1, 2) }).IsValid);
        Assert.False(validator.Validate(new OeeRunsQuery { From = new(2026, 1, 3), To = new(2026, 1, 2) }).IsValid);
        Assert.False(validator.Validate(new OeeRunsQuery { Status = (MeOne.Domain.Enums.RunStatus)9 }).IsValid);
    }

    private static class OeeMapping
    {
        public const string Default = "Padrão";
    }
}
