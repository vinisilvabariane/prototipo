using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Domain;

public sealed class ProductionTests
{
    private static readonly DateTime T0 = new(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc);

    private static LayoutPostDraft Draft(string name, PostKind kind = PostKind.Standard, bool critical = false, decimal? target = null, long id = 0) =>
        new(id, name, kind, 0, 0, 220, 140, 0, critical, target, null);

    private static LayoutDecorationDraft Deco(long id = 0) => new(id, DecorationKind.Belt, 0, 0, 300, 40, 0, "Esteira", false);

    private static LineLayout NewLayout(params LayoutPostDraft[] posts)
    {
        var layout = LineLayout.Create(1, 1, 1, 1, T0);
        layout.Id = 10;
        Assert.Null(layout.Replace(posts, [Deco()], 1, T0));

        ulong next = 100;
        foreach (var post in layout.Posts)
            post.Id = ++next;

        return layout;
    }

    private static ProductionRun NewRun(params LayoutPostDraft[] posts)
    {
        var run = ProductionRun.Create(NewLayout(posts), 1, T0);
        run.Id = 1;
        return run;
    }

    private static ulong PostId(ProductionRun run, string name) => run.Snapshot.Posts.Single(post => post.Name == name).Id;

    [Fact]
    public void Replace_AddsPostsAndDecorations()
    {
        var layout = NewLayout(Draft("A"), Draft("B", PostKind.Machine));

        Assert.Equal(["A", "B"], layout.Posts.Select(post => post.Name));
        Assert.Single(layout.Decorations);
    }

    [Fact]
    public void Replace_RejectsRepeatedNamesIgnoringCase()
    {
        var layout = LineLayout.Create(1, 1, 1, 1, T0);

        var problem = layout.Replace([Draft("Solda"), Draft(" solda ")], [], 1, T0);

        Assert.Equal(ProductionErrors.PostNameTaken("solda"), problem);
        Assert.Empty(layout.Posts);
    }

    [Fact]
    public void Replace_RejectsAPostThatBelongsToAnotherLayout()
    {
        var layout = NewLayout(Draft("A"));

        Assert.Equal(ProductionErrors.PostNotInLayout, layout.Replace([Draft("A", id: 9999)], [], 1, T0));
    }

    [Fact]
    public void Replace_KeepsUpdatesAndRemovesByIdentity()
    {
        var layout = NewLayout(Draft("A"), Draft("B"));
        var keep = (long)layout.Posts[0].Id;

        var problem = layout.Replace([Draft("A renomeado", PostKind.Machine, id: keep), Draft("C")], [], 2, T0.AddHours(1));

        Assert.Null(problem);
        Assert.Equal(2, layout.Posts.Count);
        Assert.Equal("A renomeado", layout.FindPost((ulong)keep)!.Name);
        Assert.Equal(PostKind.Machine, layout.FindPost((ulong)keep)!.Kind);
        Assert.DoesNotContain(layout.Posts, post => post.Name == "B");
        Assert.Empty(layout.Decorations);
        Assert.Equal(2UL, layout.UpdatedBy);
    }

    [Theory]
    [InlineData(10, 140, 0, null)]
    [InlineData(220, 140, 360, null)]
    [InlineData(220, 140, 0, -1)]
    public void Replace_RejectsInvalidGeometryRotationAndTarget(int width, int height, int rotation, int? target)
    {
        var layout = LineLayout.Create(1, 1, 1, 1, T0);
        var post = new LayoutPostDraft(0, "A", PostKind.Standard, 0, 0, width, height, rotation, false, target, null);

        Assert.NotNull(layout.Replace([post], [], 1, T0));
    }

    [Fact]
    public void Replace_RejectsMoreThanTheMaximumOfPosts()
    {
        var layout = LineLayout.Create(1, 1, 1, 1, T0);
        var posts = Enumerable.Range(0, LayoutRules.MaxPosts + 1).Select(index => Draft($"P{index}")).ToList();

        Assert.Equal(ProductionErrors.TooManyPosts, layout.Replace(posts, [], 1, T0));
    }

    [Fact]
    public void CopyContentFrom_ClonesTheContentWithoutTheIdentities()
    {
        var source = NewLayout(Draft("A", critical: true, target: 10), Draft("B"));
        var target = LineLayout.Create(2, 1, 1, 1, T0);
        target.Id = 11;

        target.CopyContentFrom(source, 5, T0.AddHours(2));

        Assert.Equal(["A", "B"], target.Posts.Select(post => post.Name));
        Assert.All(target.Posts, post => Assert.Equal(0UL, post.Id));
        Assert.True(target.Posts[0].IsCritical);
        Assert.Single(target.Decorations);
        Assert.Equal(5UL, target.UpdatedBy);
    }

    [Fact]
    public void Run_StartsPreparingWithASnapshotOfTheLayout()
    {
        var run = NewRun(Draft("A"), Draft("B"));

        Assert.Equal(RunStatus.Preparing, run.Status);
        Assert.Equal(2, run.Snapshot.Posts.Count);
        Assert.Single(run.Snapshot.Decorations);
        Assert.True(run.IsActive);
    }

    [Fact]
    public void CommonPost_IsExclusiveToOnePerson()
    {
        var run = NewRun(Draft("A"), Draft("B"));
        run.Allocate(PostId(run, "A"), 7, 1, T0);

        Assert.Equal(ProductionErrors.PersonAlreadyAllocated, run.CheckAllocate(PostId(run, "B"), 7, 5));
        Assert.Null(run.CheckAllocate(PostId(run, "B"), 8, 5));
    }

    [Fact]
    public void MachinePosts_AcceptTheSamePersonUpToTheLimit()
    {
        var run = NewRun(Draft("M1", PostKind.Machine), Draft("M2", PostKind.Machine), Draft("M3", PostKind.Machine));
        run.Allocate(PostId(run, "M1"), 7, 1, T0);
        run.Allocate(PostId(run, "M2"), 7, 1, T0);

        Assert.Null(run.CheckAllocate(PostId(run, "M3"), 7, 3));
        Assert.Equal(ProductionErrors.MachineLimit(2), run.CheckAllocate(PostId(run, "M3"), 7, 2));
    }

    [Fact]
    public void MachineAndCommonPosts_CannotBeMixed()
    {
        var run = NewRun(Draft("M1", PostKind.Machine), Draft("A"));
        run.Allocate(PostId(run, "M1"), 7, 1, T0);

        Assert.Equal(ProductionErrors.PersonAlreadyAllocated, run.CheckAllocate(PostId(run, "A"), 7, 5));

        var other = NewRun(Draft("M1", PostKind.Machine), Draft("A"));
        other.Allocate(PostId(other, "A"), 7, 1, T0);

        Assert.Equal(ProductionErrors.PersonHoldsCommonPost, other.CheckAllocate(PostId(other, "M1"), 7, 5));
    }

    [Fact]
    public void Allocate_ReplacesThePreviousHolderKeepingTheHistory()
    {
        var run = NewRun(Draft("A"));
        var post = PostId(run, "A");

        run.Allocate(post, 7, 1, T0);
        var changed = run.Allocate(post, 8, 1, T0.AddMinutes(30));

        Assert.True(changed);
        Assert.Equal(8UL, run.OpenAllocationOf(post)!.UserId);
        Assert.Equal(2, run.Allocations.Count);
        Assert.Equal(T0.AddMinutes(30), run.Allocations[0].ReleasedAt);
    }

    [Fact]
    public void Allocate_TheSamePersonAgain_ChangesNothing()
    {
        var run = NewRun(Draft("A"));
        var post = PostId(run, "A");
        run.Allocate(post, 7, 1, T0);

        Assert.False(run.Allocate(post, 7, 1, T0.AddMinutes(5)));
        Assert.Single(run.Allocations);
    }

    [Fact]
    public void CheckAllocate_RejectsUnknownPostsAndClosedRuns()
    {
        var run = NewRun(Draft("A", critical: false));

        Assert.Equal(ProductionErrors.PostNotInRun, run.CheckAllocate(99999, 7, 5));

        run.Allocate(PostId(run, "A"), 7, 1, T0);
        run.Start(1, T0);
        run.Close(1, T0.AddHours(1));

        Assert.Equal(ProductionErrors.RunClosed, run.CheckAllocate(PostId(run, "A"), 8, 5));
    }

    [Fact]
    public void Release_ClosesOnlyTheOpenAllocationOfThePost()
    {
        var run = NewRun(Draft("A"), Draft("B"));
        run.Allocate(PostId(run, "A"), 7, 1, T0);
        run.Allocate(PostId(run, "B"), 8, 1, T0);

        Assert.True(run.Release(PostId(run, "A"), T0.AddMinutes(10)));
        Assert.False(run.Release(PostId(run, "A"), T0.AddMinutes(11)));
        Assert.Single(run.OpenAllocations);
        Assert.Equal(1, run.ReleaseAll(T0.AddMinutes(20)));
        Assert.Empty(run.OpenAllocations);
    }

    [Fact]
    public void CheckStart_NeedsSomeoneAllocatedAndEveryCriticalPostCovered()
    {
        var run = NewRun(Draft("A", critical: true), Draft("B"));

        Assert.Equal(ProductionErrors.RunWithoutAllocations, run.CheckStart());

        run.Allocate(PostId(run, "B"), 7, 1, T0);
        Assert.Equal(ProductionErrors.CriticalPostsEmpty(["A"]), run.CheckStart());

        run.Allocate(PostId(run, "A"), 8, 1, T0);
        Assert.Null(run.CheckStart());
    }

    [Fact]
    public void StartAndClose_MoveTheStatusAndReleaseEveryone()
    {
        var run = NewRun(Draft("A"));
        run.Allocate(PostId(run, "A"), 7, 1, T0);

        run.Start(2, T0.AddMinutes(5));
        Assert.Equal(RunStatus.Running, run.Status);
        Assert.Equal(2UL, run.StartedBy);
        Assert.Equal(ProductionErrors.RunNotPreparing, run.CheckStart());
        Assert.Equal(ProductionErrors.RunCannotBeCancelled, run.CheckCancel());

        run.Close(3, T0.AddHours(2));
        Assert.Equal(RunStatus.Closed, run.Status);
        Assert.Equal(3UL, run.EndedBy);
        Assert.Empty(run.OpenAllocations);
        Assert.Equal(T0.AddHours(2), run.Allocations[0].ReleasedAt);
        Assert.Equal(ProductionErrors.RunNotRunning, run.CheckClose());
    }

    [Fact]
    public void CheckRecordOutput_EnforcesTheStateTheQuantityAndTheHourWindow()
    {
        var run = NewRun(Draft("A"));
        var post = PostId(run, "A");

        Assert.Equal(ProductionErrors.RunNotRunning, run.CheckRecordOutput(post, 10, T0, T0));

        run.Allocate(post, 7, 1, T0);
        run.Start(1, T0.AddMinutes(10));

        Assert.Equal(ProductionErrors.PostNotInRun, run.CheckRecordOutput(99999, 10, T0, T0.AddHours(1)));
        Assert.Equal(ProductionErrors.QuantityInvalid, run.CheckRecordOutput(post, -1, T0, T0.AddHours(1)));
        Assert.Equal(ProductionErrors.OutputInTheFuture, run.CheckRecordOutput(post, 10, T0.AddHours(3), T0.AddHours(1)));
        Assert.Equal(ProductionErrors.OutputBeforeStart, run.CheckRecordOutput(post, 10, T0.AddHours(-1), T0.AddHours(1)));
        Assert.Null(run.CheckRecordOutput(post, 10, T0.AddMinutes(20), T0.AddHours(1)));
    }

    [Fact]
    public void RecordOutput_UpsertsTheHourAndCreditsTheHolder()
    {
        var run = NewRun(Draft("A"));
        var post = PostId(run, "A");
        run.Allocate(post, 7, 1, T0);
        run.Start(1, T0);

        var first = run.RecordOutput(post, 40, T0.AddMinutes(15), 1, T0.AddMinutes(30));
        var again = run.RecordOutput(post, 55, T0.AddMinutes(45), 2, T0.AddMinutes(50));

        Assert.Same(first, again);
        Assert.Single(run.Outputs);
        Assert.Equal(55, again.Quantity);
        Assert.Equal(7UL, again.UserId);
        Assert.Equal(T0, again.HourStart);
    }

    [Fact]
    public void RecordOutput_OfAPastHourCreditsWhoHeldThePostThen()
    {
        var run = NewRun(Draft("A"));
        var post = PostId(run, "A");
        run.Allocate(post, 7, 1, T0);
        run.Start(1, T0);
        run.Allocate(post, 8, 1, T0.AddHours(1));

        var past = run.RecordOutput(post, 30, T0, 1, T0.AddHours(2));
        var current = run.RecordOutput(post, 20, T0.AddHours(1), 1, T0.AddHours(2));

        Assert.Equal(7UL, past.UserId);
        Assert.Equal(8UL, current.UserId);
    }

    [Fact]
    public void CopyAllocationsFrom_MatchesPostsByNameAndSkipsWhoIsNotFree()
    {
        var previous = NewRun(Draft("A"), Draft("B"), Draft("C"));
        previous.Allocate(PostId(previous, "A"), 7, 1, T0);
        previous.Allocate(PostId(previous, "B"), 8, 1, T0);
        previous.Allocate(PostId(previous, "C"), 9, 1, T0);
        previous.Start(1, T0);
        previous.Close(1, T0.AddHours(8));

        var next = NewRun(Draft("a"), Draft("B"));

        var (allocated, skipped) = next.CopyAllocationsFrom(previous, userId => userId != 8, 5, 1, T0.AddDays(1));

        Assert.Equal(1, allocated);
        Assert.Equal(2, skipped);
        Assert.Equal(7UL, next.OpenAllocationOf(PostId(next, "a"))!.UserId);
        Assert.Null(next.OpenAllocationOf(PostId(next, "B")));
    }

    [Fact]
    public void Productivity_ComparesTheProducedQuantityWithTheTargetForTheTimeOccupied()
    {
        var run = NewRun(Draft("A", target: 100), Draft("B"));
        var a = PostId(run, "A");
        run.Allocate(a, 7, 1, T0);
        run.Start(1, T0);
        run.RecordOutput(a, 150, T0, 1, T0.AddMinutes(50));

        var report = RunProductivity.Compute(run, T0.AddHours(2));

        var post = report.Posts.Single(item => item.PostId == a);
        Assert.Equal(120, post.OccupiedMinutes);
        Assert.Equal(150, post.Produced);
        Assert.Equal(200m, post.Expected);
        Assert.Equal(0.75m, post.Attainment);
        Assert.Equal(150, report.TotalProduced);
        Assert.Equal(7UL, report.People.Single().UserId);
        Assert.Equal(150, report.People.Single().Produced);
        Assert.Equal(T0, report.Hours.Single().HourStart);

        var untouched = report.Posts.Single(item => item.PostId != a);
        Assert.Equal(0, untouched.OccupiedMinutes);
        Assert.Null(untouched.Expected);
        Assert.Null(untouched.Attainment);
    }

    [Fact]
    public void Productivity_CountsAPersonOnTwoMachinesOnlyOnce()
    {
        var run = NewRun(Draft("M1", PostKind.Machine, target: 60), Draft("M2", PostKind.Machine, target: 60));
        var m1 = PostId(run, "M1");
        var m2 = PostId(run, "M2");
        run.Allocate(m1, 7, 1, T0);
        run.Allocate(m2, 7, 1, T0.AddMinutes(30));
        run.Start(1, T0);
        run.Close(1, T0.AddMinutes(90));

        var report = RunProductivity.Compute(run, T0.AddHours(5));

        Assert.Equal(90, report.Posts.Single(item => item.PostId == m1).OccupiedMinutes);
        Assert.Equal(60, report.Posts.Single(item => item.PostId == m2).OccupiedMinutes);
        Assert.Equal(90, report.People.Single().Minutes);
    }

    [Fact]
    public void Productivity_IgnoresTimeBeforeTheProductionStarted()
    {
        var run = NewRun(Draft("A", target: 60));
        var a = PostId(run, "A");
        run.Allocate(a, 7, 1, T0);
        run.Start(1, T0.AddMinutes(30));

        var report = RunProductivity.Compute(run, T0.AddMinutes(90));

        Assert.Equal(60, report.Posts.Single().OccupiedMinutes);
    }

    [Fact]
    public void Productivity_OfARunThatNeverStarted_IsEmpty()
    {
        var run = NewRun(Draft("A", target: 60));
        run.Allocate(PostId(run, "A"), 7, 1, T0);

        var report = RunProductivity.Compute(run, T0.AddHours(1));

        Assert.Equal(0, report.TotalProduced);
        Assert.Empty(report.People);
        Assert.Equal(0, report.Posts.Single().OccupiedMinutes);
    }
}
