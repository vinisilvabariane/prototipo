using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Oee;
using MeOne.Application.UseCases.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Tests.Fakes;

public sealed class ProductionFixture
{
    public ProductionFixture()
    {
        Access = new AccessFixture();
        Supervisor = Access.AddUser(3201, "supervisor@example.com", "Sonia Supervisora");
        Ana = Access.AddUser(3202, "ana@example.com", "Ana Operadora");
        Bruno = Access.AddUser(3203, "bruno@example.com", "Bruno Operador");
        Carla = Access.AddUser(3204, "carla@example.com", "Carla Operadora");

        Line = ProductionLine.Create("Linha 1", DateTime.UtcNow);
        Shift = ProductionShift.Create("Turno 1", new TimeOnly(6, 0), new TimeOnly(14, 0));
        Product = ProductionProduct.Create("p-100", "Fone X");

        Lines.Insert(Line);
        Shifts.Insert(Shift);
        Products.Insert(Product);
    }

    public AccessFixture Access { get; }

    public FakeProductionLineRepository Lines { get; } = new();

    public FakeProductionShiftRepository Shifts { get; } = new();

    public FakeProductionProductRepository Products { get; } = new();

    public FakeLineLayoutRepository Layouts { get; } = new();

    public FakeProductionRunRepository Runs { get; } = new();

    public FakeOeeFormulaSetRepository FormulaSets { get; } = new();

    public FakeOeeLossSource LossSource { get; } = new();

    public User Supervisor { get; }

    public User Ana { get; }

    public User Bruno { get; }

    public User Carla { get; }

    public ProductionLine Line { get; }

    public ProductionShift Shift { get; }

    public ProductionProduct Product { get; }

    public FakeCurrentActor SupervisorActor => FakeCurrentActor.For(Supervisor);

    public FakeCurrentActor FullAccessActor => FakeCurrentActor.WithPermissions(
        Supervisor.Id,
        MeOne.Domain.Rules.Permissions.EngineeringManage,
        MeOne.Domain.Rules.Permissions.ProductionSupervise,
        MeOne.Domain.Rules.Permissions.ProductionExecute);

    public CreateLayoutUseCase CreateLayout => new(Layouts, Lines, Products, Shifts, SupervisorActor);

    public SaveLayoutUseCase SaveLayout => new(Layouts, SupervisorActor);

    public CopyLayoutUseCase CopyLayout => new(Layouts, Lines, Products, Shifts, SupervisorActor);

    public DeleteLayoutUseCase DeleteLayout => new(Layouts);

    public ListLayoutsUseCase ListLayouts(ICurrentActor? actor = null) => new(Layouts, Lines, Products, Shifts, actor ?? SupervisorActor);

    public GetLayoutUseCase GetLayout(ICurrentActor? actor = null) => new(Layouts, Lines, Products, Shifts, Access.Users, actor ?? SupervisorActor);

    public CreateRunUseCase CreateRun => new(Runs, Layouts, Lines, Products, Shifts, SupervisorActor);

    public AllocatePostUseCase Allocate => new(Runs, Lines, Access.Users, SupervisorActor);

    public ReleasePostUseCase Release => new(Runs);

    public ReleaseAllPostsUseCase ReleaseAll => new(Runs);

    public CopyPreviousAllocationsUseCase CopyPrevious => new(Runs, Access.Users, SupervisorActor);

    public StartRunUseCase Start => new(Runs, SupervisorActor);

    public CloseRunUseCase Close => new(Runs, SupervisorActor);

    public CancelRunUseCase Cancel => new(Runs, SupervisorActor);

    public RecordRunOutputUseCase RecordOutput => new(Runs, SupervisorActor);

    public ListRunCandidatesUseCase Candidates => new(Runs, Lines, Access.Users);

    public ListRunsUseCase ListRuns(ICurrentActor? actor = null) => new(Runs, Lines, Products, Shifts, actor ?? SupervisorActor);

    public GetRunUseCase GetRun(ICurrentActor? actor = null) => new(Runs, Lines, Products, Shifts, Access.Users, actor ?? SupervisorActor);

    public GetRunOeeUseCase GetRunOee => new(Runs, Lines, Products, Shifts, FormulaSets, LossSource);

    public ListOeeRunsUseCase ListOeeRuns => new(Runs, Lines, Products, Shifts, FormulaSets, LossSource);

    public GetOeeConfigUseCase GetOeeConfig(ICurrentActor? actor = null) => new(FormulaSets, Access.Users, LossSource, actor ?? SupervisorActor);

    public CreateOeeFormulaSetUseCase CreateFormulaSet => new(FormulaSets, Access.Users, SupervisorActor);

    public UpdateOeeFormulaSetUseCase UpdateFormulaSet => new(FormulaSets, SupervisorActor);

    public DeleteOeeFormulaSetUseCase DeleteFormulaSet => new(FormulaSets);

    public SetActiveOeeFormulaSetUseCase SetActiveFormulaSet => new(FormulaSets);

    public TestOeeFormulaUseCase TestFormula => new(Runs, FormulaSets, LossSource);

    public static SaveLayoutPostRequest Post(
        string name,
        PostKind kind = PostKind.Standard,
        int x = 0,
        int y = 0,
        bool critical = false,
        decimal? target = null,
        long id = 0) =>
        new(id, name, kind, x, y, 220, 140, 0, critical, target, null);

    public static SaveLayoutDecorationRequest Decoration(DecorationKind kind = DecorationKind.Belt, long id = 0) =>
        new(id, kind, 30, 30, 300, 40, 0, "Esteira", false);

    public static SaveLayoutRequest Layout(IReadOnlyList<SaveLayoutPostRequest>? posts = null, IReadOnlyList<SaveLayoutDecorationRequest>? decorations = null) =>
        new(posts ?? DefaultPosts(), decorations ?? []);

    public static IReadOnlyList<SaveLayoutPostRequest> DefaultPosts() =>
    [
        Post("Montagem", PostKind.Standard, 0, 0, critical: true, target: 100),
        Post("Maquina A", PostKind.Machine, 300, 0, target: 50),
        Post("Maquina B", PostKind.Machine, 600, 0, target: 50),
        Post("Qualidade", PostKind.Quality, 900, 0, target: 120),
    ];

    public async Task<LineLayout> LayoutAsync(SaveLayoutRequest? content = null, ProductionLine? line = null)
    {
        var target = line ?? Line;
        var created = await CreateLayout.ExecuteAsync(new CreateLayoutRequest(target.Id, Product.Id, Shift.Id), CancellationToken.None);
        Assert.True(created.IsSuccess, string.Join(";", created.Errors.Select(error => error.Message)));

        var saved = await SaveLayout.ExecuteAsync(created.Value.Id, content ?? Layout(), CancellationToken.None);
        Assert.True(saved.IsSuccess, string.Join(";", saved.Errors.Select(error => error.Message)));

        return Layouts.Stored.Single(layout => layout.Id == created.Value.Id);
    }

    public async Task<ProductionRun> RunAsync(LineLayout? layout = null)
    {
        var source = layout ?? Layouts.Stored.FirstOrDefault(candidate => candidate.LineId == Line.Id) ?? await LayoutAsync();
        var created = await CreateRun.ExecuteAsync(new CreateRunRequest(source.Id), CancellationToken.None);
        Assert.True(created.IsSuccess, string.Join(";", created.Errors.Select(error => error.Message)));

        return Runs.Stored.Single(run => run.Id == created.Value.Id);
    }

    public ulong PostId(ProductionRun run, string name) =>
        run.Snapshot.Posts.Single(post => post.Name == name).Id;

    public async Task AllocateAsync(ProductionRun run, string post, User user)
    {
        var result = await Allocate.ExecuteAsync(run.Id, PostId(run, post), new AllocatePostRequest(user.Id), CancellationToken.None);
        Assert.True(result.IsSuccess, string.Join(";", result.Errors.Select(error => error.Message)));
    }

    public async Task<ProductionRun> StartedRunAsync()
    {
        var run = await RunAsync();
        await AllocateAsync(run, "Montagem", Ana);
        var started = await Start.ExecuteAsync(run.Id, CancellationToken.None);
        Assert.True(started.IsSuccess, string.Join(";", started.Errors.Select(error => error.Message)));
        return run;
    }
}
