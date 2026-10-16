using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;
using static MeOne.Tests.Fakes.ProductionFixture;

namespace MeOne.Tests.Application;

public sealed class ProductionUseCaseTests
{
    private static string Message(FluentResults.ResultBase result) => Assert.Single(result.Errors).Message;

    [Fact]
    public async Task Line_IsCreatedUpdatedAndKeepsItsNameUnique()
    {
        var f = new ProductionFixture();
        var create = new CreateProductionLineUseCase(f.Lines);
        var update = new UpdateProductionLineUseCase(f.Lines);

        var created = await create.ExecuteAsync(new SaveProductionLineRequest(" Linha 2 ", true), CancellationToken.None);
        var duplicate = await create.ExecuteAsync(new SaveProductionLineRequest("linha 2", true), CancellationToken.None);
        var renamed = await update.ExecuteAsync(created.Value.Id, new SaveProductionLineRequest("Linha 2B", false), CancellationToken.None);
        var clash = await update.ExecuteAsync(created.Value.Id, new SaveProductionLineRequest("Linha 1", true), CancellationToken.None);
        var missing = await update.ExecuteAsync(999, new SaveProductionLineRequest("X", true), CancellationToken.None);

        Assert.Equal("Linha 2", created.Value.Name);
        Assert.Equal(ProductionErrors.NameTaken("linha 2"), Message(duplicate));
        Assert.True(renamed.IsSuccess);
        Assert.False(f.Lines.Stored.Single(line => line.Id == created.Value.Id).Active);
        Assert.Equal(ProductionErrors.NameTaken("Linha 1"), Message(clash));
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
    }

    [Fact]
    public async Task Line_ListHidesInactiveOnesUnlessAsked()
    {
        var f = new ProductionFixture();
        var old = await new CreateProductionLineUseCase(f.Lines).ExecuteAsync(new SaveProductionLineRequest("Linha antiga", true), CancellationToken.None);
        await new UpdateProductionLineUseCase(f.Lines).ExecuteAsync(old.Value.Id, new SaveProductionLineRequest("Linha antiga", false), CancellationToken.None);
        var list = new ListProductionLinesUseCase(f.Lines);

        var active = await list.ExecuteAsync(new SetupQuery(), CancellationToken.None);
        var all = await list.ExecuteAsync(new SetupQuery { IncludeInactive = true }, CancellationToken.None);

        Assert.Equal(["Linha 1"], active.Value.Select(line => line.Name));
        Assert.Equal(2, all.Value.Count);
    }

    [Fact]
    public async Task Shift_AndProduct_KeepTheirKeysUnique()
    {
        var f = new ProductionFixture();
        var shift = await new CreateProductionShiftUseCase(f.Shifts).ExecuteAsync(new SaveProductionShiftRequest("Turno 1", new(6, 0), new(14, 0), true), CancellationToken.None);
        var product = await new CreateProductionProductUseCase(f.Products).ExecuteAsync(new SaveProductionProductRequest(" p-100 ", "Outro", true), CancellationToken.None);
        var fresh = await new CreateProductionProductUseCase(f.Products).ExecuteAsync(new SaveProductionProductRequest("p-200", "Novo", true), CancellationToken.None);

        Assert.Equal(ProductionErrors.NameTaken("Turno 1"), Message(shift));
        Assert.Equal(ProductionErrors.CodeTaken("P-100"), Message(product));
        Assert.Equal("P-200", fresh.Value.Code);
    }

    [Fact]
    public async Task CreateLayout_RejectsInactiveReferencesAndDuplicates()
    {
        var f = new ProductionFixture();
        await f.CreateLayout.ExecuteAsync(new CreateLayoutRequest(f.Line.Id, f.Product.Id, f.Shift.Id), CancellationToken.None);

        var duplicate = await f.CreateLayout.ExecuteAsync(new CreateLayoutRequest(f.Line.Id, f.Product.Id, f.Shift.Id), CancellationToken.None);
        var unknown = await f.CreateLayout.ExecuteAsync(new CreateLayoutRequest(999, f.Product.Id, f.Shift.Id), CancellationToken.None);

        await new UpdateProductionLineUseCase(f.Lines).ExecuteAsync(f.Line.Id, new SaveProductionLineRequest("Linha 1", false), CancellationToken.None);
        var inactive = await f.CreateLayout.ExecuteAsync(new CreateLayoutRequest(f.Line.Id, f.Product.Id, f.Shift.Id), CancellationToken.None);

        Assert.Equal(ProductionErrors.LayoutTaken("Linha 1", "P-100", "Turno 1"), Message(duplicate));
        Assert.IsType<NotFoundError>(Assert.Single(unknown.Errors));
        Assert.Equal(ProductionErrors.LineInactive, Message(inactive));
    }

    [Fact]
    public async Task SaveLayout_StoresThePostsAndDecorationsAndKeepsThePostIdentities()
    {
        var f = new ProductionFixture();
        var layout = await f.LayoutAsync(Layout(decorations: [Decoration()]));
        var montagem = layout.Posts.Single(post => post.Name == "Montagem");

        var again = await f.SaveLayout.ExecuteAsync(
            layout.Id,
            Layout(
            [
                Post("Montagem final", PostKind.Standard, 30, 30, critical: true, target: 90, id: (long)montagem.Id),
                Post("Embalagem", PostKind.Standard, 600, 30),
            ]),
            CancellationToken.None);

        Assert.True(again.IsSuccess);
        Assert.Equal(["Montagem final", "Embalagem"], layout.Posts.Select(post => post.Name));
        Assert.Equal(montagem.Id, layout.Posts[0].Id);
        Assert.Empty(layout.Decorations);
        Assert.Equal(f.Supervisor.Id, layout.UpdatedBy);
    }

    [Fact]
    public async Task SaveLayout_ExplainsWhatIsWrong()
    {
        var f = new ProductionFixture();
        var layout = await f.LayoutAsync();

        var duplicated = await f.SaveLayout.ExecuteAsync(layout.Id, Layout([Post("A"), Post("a")]), CancellationToken.None);
        var missing = await f.SaveLayout.ExecuteAsync(999, Layout(), CancellationToken.None);

        Assert.Equal(ProductionErrors.PostNameTaken("a"), Message(duplicated));
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
        Assert.Equal(4, layout.Posts.Count);
    }

    [Fact]
    public async Task CopyLayout_CreatesNewTargetsAndOverwritesExistingOnes()
    {
        var f = new ProductionFixture();
        var source = await f.LayoutAsync();
        var line2 = (await new CreateProductionLineUseCase(f.Lines).ExecuteAsync(new SaveProductionLineRequest("Linha 2", true), CancellationToken.None)).Value;
        var shift2 = (await new CreateProductionShiftUseCase(f.Shifts).ExecuteAsync(new SaveProductionShiftRequest("Turno 2", new(14, 0), new(22, 0), true), CancellationToken.None)).Value;

        var first = await f.CopyLayout.ExecuteAsync(
            source.Id,
            new CopyLayoutRequest([new(line2.Id, f.Product.Id, f.Shift.Id), new(f.Line.Id, f.Product.Id, shift2.Id)]),
            CancellationToken.None);

        await f.SaveLayout.ExecuteAsync(source.Id, Layout([Post("Unico")]), CancellationToken.None);

        var second = await f.CopyLayout.ExecuteAsync(
            source.Id,
            new CopyLayoutRequest([new(line2.Id, f.Product.Id, f.Shift.Id)]),
            CancellationToken.None);

        Assert.Equal((2, 0), (first.Value.Created, first.Value.Updated));
        Assert.Equal((0, 1), (second.Value.Created, second.Value.Updated));
        Assert.Equal(3, f.Layouts.Stored.Count);
        var copy = f.Layouts.Stored.Single(layout => layout.LineId == line2.Id);
        Assert.Equal(["Unico"], copy.Posts.Select(post => post.Name));
    }

    [Fact]
    public async Task CopyLayout_RejectsEmptyRepeatedAndSelfTargets()
    {
        var f = new ProductionFixture();
        var source = await f.LayoutAsync();
        var self = new LayoutTargetRequest(f.Line.Id, f.Product.Id, f.Shift.Id);

        var empty = await f.CopyLayout.ExecuteAsync(source.Id, new CopyLayoutRequest([]), CancellationToken.None);
        var toSelf = await f.CopyLayout.ExecuteAsync(source.Id, new CopyLayoutRequest([self]), CancellationToken.None);
        var repeated = await f.CopyLayout.ExecuteAsync(source.Id, new CopyLayoutRequest([new(2, 1, 1), new(2, 1, 1)]), CancellationToken.None);

        Assert.Equal(ProductionErrors.CopyTargetsRequired, Message(empty));
        Assert.Equal(ProductionErrors.CopyToItself, Message(toSelf));
        Assert.Equal(ProductionErrors.CopyTargetRepeated, Message(repeated));
    }

    [Fact]
    public async Task Layouts_AreListedWithNamesAndCanBeDeleted()
    {
        var f = new ProductionFixture();
        var layout = await f.LayoutAsync(Layout(decorations: [Decoration()]));

        var list = await f.ListLayouts().ExecuteAsync(new LayoutsQuery(), CancellationToken.None);
        var detail = await f.GetLayout().ExecuteAsync(layout.Id, CancellationToken.None);
        var deleted = await f.DeleteLayout.ExecuteAsync(layout.Id, CancellationToken.None);

        var item = Assert.Single(list.Value.Items);
        Assert.Equal(("Linha 1", "P-100", "Turno 1", 4), (item.LineName, item.ProductCode, item.ShiftName, item.PostCount));
        Assert.Equal(f.Supervisor.Name, detail.Value.UpdatedBy);
        Assert.Equal(4, detail.Value.Posts.Count);
        Assert.Single(detail.Value.Decorations);
        Assert.True(deleted.IsSuccess);
        Assert.Empty(f.Layouts.Stored);
    }

    [Fact]
    public async Task CreateRun_SnapshotsTheLayoutAndBlocksASecondRunOnTheSameLine()
    {
        var f = new ProductionFixture();
        var layout = await f.LayoutAsync();

        var run = await f.RunAsync(layout);
        await f.SaveLayout.ExecuteAsync(layout.Id, Layout([Post("Outro")]), CancellationToken.None);
        var second = await f.CreateRun.ExecuteAsync(new CreateRunRequest(layout.Id), CancellationToken.None);

        Assert.Equal(RunStatus.Preparing, run.Status);
        Assert.Equal(4, run.Snapshot.Posts.Count);
        Assert.Equal(ProductionErrors.LineBusy("Linha 1"), Message(second));
    }

    [Fact]
    public async Task Allocate_PutsAPersonOnAPostAndRejectsUnknownOnes()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();
        var montagem = f.PostId(run, "Montagem");

        var ok = await f.Allocate.ExecuteAsync(run.Id, montagem, new AllocatePostRequest(f.Ana.Id), CancellationToken.None);
        var noUser = await f.Allocate.ExecuteAsync(run.Id, montagem, new AllocatePostRequest(9999), CancellationToken.None);
        var noPost = await f.Allocate.ExecuteAsync(run.Id, 9999, new AllocatePostRequest(f.Bruno.Id), CancellationToken.None);
        var noRun = await f.Allocate.ExecuteAsync(9999, montagem, new AllocatePostRequest(f.Bruno.Id), CancellationToken.None);

        Assert.True(ok.IsSuccess);
        Assert.Equal(f.Ana.Id, run.OpenAllocationOf(montagem)!.UserId);
        Assert.IsType<NotFoundError>(Assert.Single(noUser.Errors));
        Assert.IsType<NotFoundError>(Assert.Single(noPost.Errors));
        Assert.IsType<NotFoundError>(Assert.Single(noRun.Errors));
    }

    [Fact]
    public async Task Allocate_AppliesTheMachineAndCommonPostRules()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();

        await f.AllocateAsync(run, "Maquina A", f.Ana);
        await f.AllocateAsync(run, "Maquina B", f.Ana);
        var common = await f.Allocate.ExecuteAsync(run.Id, f.PostId(run, "Montagem"), new AllocatePostRequest(f.Ana.Id), CancellationToken.None);

        Assert.Equal(ProductionErrors.PersonAlreadyAllocated, Message(common));
        Assert.Equal(2, run.OpenAllocations.Count(allocation => allocation.UserId == f.Ana.Id));
    }

    [Fact]
    public async Task Allocate_RefusesSomeoneBusyOnAnotherLinesProduction()
    {
        var f = new ProductionFixture();
        var line2 = (await new CreateProductionLineUseCase(f.Lines).ExecuteAsync(new SaveProductionLineRequest("Linha 2", true), CancellationToken.None)).Value;
        var elsewhere = await f.RunAsync(await f.LayoutAsync(line: f.Lines.Stored.Single(line => line.Id == line2.Id)));
        await f.AllocateAsync(elsewhere, "Montagem", f.Ana);

        var layoutHere = await f.LayoutAsync();
        var here = await f.RunAsync(layoutHere);

        var result = await f.Allocate.ExecuteAsync(here.Id, f.PostId(here, "Montagem"), new AllocatePostRequest(f.Ana.Id), CancellationToken.None);

        Assert.Equal(ProductionErrors.PersonBusy(f.Ana.Name, "Linha 2"), Message(result));
        Assert.Empty(here.OpenAllocations);
    }

    [Fact]
    public async Task Release_FreesOnePostOrAllOfThem()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();
        await f.AllocateAsync(run, "Montagem", f.Ana);
        await f.AllocateAsync(run, "Qualidade", f.Bruno);

        await f.Release.ExecuteAsync(run.Id, f.PostId(run, "Montagem"), CancellationToken.None);
        Assert.Single(run.OpenAllocations);

        await f.ReleaseAll.ExecuteAsync(run.Id, CancellationToken.None);
        Assert.Empty(run.OpenAllocations);
    }

    [Fact]
    public async Task Start_RefusesWhileACriticalPostIsEmptyThenRuns()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();
        await f.AllocateAsync(run, "Qualidade", f.Bruno);

        var refused = await f.Start.ExecuteAsync(run.Id, CancellationToken.None);
        await f.AllocateAsync(run, "Montagem", f.Ana);
        var started = await f.Start.ExecuteAsync(run.Id, CancellationToken.None);

        Assert.Equal(ProductionErrors.CriticalPostsEmpty(["Montagem"]), Message(refused));
        Assert.True(started.IsSuccess);
        Assert.Equal(RunStatus.Running, run.Status);
        Assert.Equal(f.Supervisor.Id, run.StartedBy);
    }

    [Fact]
    public async Task Cancel_OnlyWorksBeforeTheProductionStarts()
    {
        var f = new ProductionFixture();
        var preparing = await f.RunAsync();
        var cancelled = await f.Cancel.ExecuteAsync(preparing.Id, CancellationToken.None);

        var running = await f.StartedRunAsync();
        var refused = await f.Cancel.ExecuteAsync(running.Id, CancellationToken.None);

        Assert.True(cancelled.IsSuccess);
        Assert.DoesNotContain(f.Runs.Stored, run => run.Id == preparing.Id);
        Assert.Equal(ProductionErrors.RunCannotBeCancelled, Message(refused));
    }

    [Fact]
    public async Task RecordOutput_NeedsARunningProductionAndAValidQuantity()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();
        var montagem = f.PostId(run, "Montagem");
        var early = await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(montagem, 10, null), CancellationToken.None);

        await f.AllocateAsync(run, "Montagem", f.Ana);
        await f.Start.ExecuteAsync(run.Id, CancellationToken.None);

        var ok = await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(montagem, 80, null), CancellationToken.None);
        var future = await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(montagem, 80, DateTime.UtcNow.AddHours(3)), CancellationToken.None);
        var unknown = await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(9999, 80, null), CancellationToken.None);

        Assert.Equal(ProductionErrors.RunNotRunning, Message(early));
        Assert.True(ok.IsSuccess);
        Assert.Equal(80, run.Outputs.Single().Quantity);
        Assert.Equal(f.Ana.Id, run.Outputs.Single().UserId);
        Assert.Equal(ProductionErrors.OutputInTheFuture, Message(future));
        Assert.IsType<NotFoundError>(Assert.Single(unknown.Errors));
    }

    [Fact]
    public async Task Close_ReleasesEveryoneAndTheDetailReportsTheProductivity()
    {
        var f = new ProductionFixture();
        var run = await f.StartedRunAsync();
        var montagem = f.PostId(run, "Montagem");
        await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(montagem, 60, null), CancellationToken.None);

        var live = await f.GetRun().ExecuteAsync(run.Id, CancellationToken.None);
        var closed = await f.Close.ExecuteAsync(run.Id, CancellationToken.None);
        var after = await f.GetRun().ExecuteAsync(run.Id, CancellationToken.None);

        Assert.Equal(f.Ana.Name, live.Value.Posts.Single(post => post.Id == montagem).Allocation!.UserName);
        Assert.Equal(60, live.Value.TotalProduced);
        Assert.Equal(f.Supervisor.Name, live.Value.CreatedBy);
        Assert.True(closed.IsSuccess);
        Assert.Equal(RunStatus.Closed, after.Value.Status);
        Assert.All(after.Value.Posts, post => Assert.Null(post.Allocation));
        var person = Assert.Single(after.Value.People);
        Assert.Equal((f.Ana.Name, 60), (person.UserName, person.Produced));
        Assert.Equal(60, Assert.Single(after.Value.Hours).Produced);
    }

    [Fact]
    public async Task CopyPrevious_RepeatsWhoWorkedTheLastProduction()
    {
        var f = new ProductionFixture();
        var layout = await f.LayoutAsync();
        var first = await f.RunAsync(layout);
        await f.AllocateAsync(first, "Montagem", f.Ana);
        await f.AllocateAsync(first, "Qualidade", f.Bruno);
        await f.Start.ExecuteAsync(first.Id, CancellationToken.None);
        await f.Close.ExecuteAsync(first.Id, CancellationToken.None);

        var second = await f.RunAsync(layout);
        var copied = await f.CopyPrevious.ExecuteAsync(second.Id, CancellationToken.None);

        Assert.Equal((2, 0), (copied.Value.Allocated, copied.Value.Skipped));
        Assert.Equal(f.Ana.Id, second.OpenAllocationOf(f.PostId(second, "Montagem"))!.UserId);
        Assert.Equal(f.Bruno.Id, second.OpenAllocationOf(f.PostId(second, "Qualidade"))!.UserId);
    }

    [Fact]
    public async Task CopyPrevious_WithoutAClosedProduction_IsNotFound()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();

        var result = await f.CopyPrevious.ExecuteAsync(run.Id, CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task Candidates_ShowWhoIsAlreadyOnMachinesAndWhoIsBusyElsewhere()
    {
        var f = new ProductionFixture();
        var run = await f.RunAsync();
        await f.AllocateAsync(run, "Maquina A", f.Ana);

        var all = await f.Candidates.ExecuteAsync(run.Id, new CandidatesQuery(), CancellationToken.None);
        var filtered = await f.Candidates.ExecuteAsync(run.Id, new CandidatesQuery { Search = "bruno" }, CancellationToken.None);

        var ana = all.Value.Single(candidate => candidate.Id == f.Ana.Id);
        Assert.Equal(1, ana.MachinePosts);
        Assert.False(ana.HoldsCommonPost);
        Assert.Null(ana.BusyLine);
        Assert.Equal(f.Bruno.Id, Assert.Single(filtered.Value).Id);
    }

    [Fact]
    public async Task Runs_AreListedNewestFirstWithCountsAndFilteredByStatus()
    {
        var f = new ProductionFixture();
        var run = await f.StartedRunAsync();
        await f.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(f.PostId(run, "Montagem"), 25, null), CancellationToken.None);

        var all = await f.ListRuns().ExecuteAsync(new RunsQuery(), CancellationToken.None);
        var closed = await f.ListRuns().ExecuteAsync(new RunsQuery { Status = RunStatus.Closed }, CancellationToken.None);

        var item = Assert.Single(all.Value.Items);
        Assert.Equal((RunStatus.Running, 1, 4, 25), (item.Status, item.AllocatedPosts, item.TotalPosts, item.Produced));
        Assert.Empty(closed.Value.Items);
    }
}
