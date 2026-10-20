using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Oee;

namespace MeOne.Tests.Fakes;

public sealed class FakeHelpdeskSectorRepository : InMemoryRepository<HelpdeskSector, ulong>, IHelpdeskSectorRepository
{
    public Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase) && item.Id != exceptId));

    public Task<IReadOnlyList<HelpdeskSector>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<HelpdeskSector>>([.. Items.Where(item => includeInactive || item.Active).OrderBy(item => item.Name)]);

    public Task<IReadOnlyList<HelpdeskSector>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<HelpdeskSector>>([.. Items.Where(item => ids.Contains(item.Id))]);
}

public sealed class FakeHelpdeskFieldTemplateRepository : InMemoryRepository<HelpdeskFieldTemplate, ulong>, IHelpdeskFieldTemplateRepository
{
    public Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase) && item.Id != exceptId));

    public Task<IReadOnlyList<HelpdeskFieldTemplate>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<HelpdeskFieldTemplate>>([.. Items.OrderBy(item => item.Name)]);
}

public sealed class FakeHelpdeskTicketRepository : InMemoryRepository<HelpdeskTicket, ulong>, IHelpdeskTicketRepository
{
    public Task<int> CountAsync(HelpdeskTicketFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult(Filtered(filter).Count());

    public Task<IReadOnlyDictionary<HelpdeskTicketStatus, int>> CountByStatusAsync(HelpdeskTicketFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<HelpdeskTicketStatus, int>>(
            Filtered(filter).GroupBy(ticket => ticket.Status).ToDictionary(group => group.Key, group => group.Count()));

    public Task<IReadOnlyList<HelpdeskTicket>> SearchAsync(HelpdeskTicketFilter filter, int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<HelpdeskTicket>>(
            [.. Filtered(filter).OrderByDescending(ticket => ticket.OpenedAt).ThenByDescending(ticket => ticket.Id).Skip(skip).Take(take)]);

    public Task<IReadOnlyList<HelpdeskTicket>> ListLineLossesAsync(ulong lineId, DateTime from, DateTime to, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<HelpdeskTicket>>(
            [.. Items.Where(ticket => ticket.LineId == lineId && ticket.Status != HelpdeskTicketStatus.Rejected)
                .Where(ticket =>
                    (ticket.IsLineStopped && ticket.StoppedAt != null && ticket.StoppedAt <= to && (ticket.FinishedAt == null || ticket.FinishedAt >= from))
                    || (ticket.Defects > 0 && ticket.OpenedAt >= from && ticket.OpenedAt <= to))]);

    public Task<bool> SectorHasTicketsAsync(ulong sectorId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(ticket => ticket.SectorId == sectorId));

    private IEnumerable<HelpdeskTicket> Filtered(HelpdeskTicketFilter filter) =>
        Items
            .Where(ticket => filter.SectorIds is null || filter.SectorIds.Contains(ticket.SectorId))
            .Where(ticket => filter.OpenedBy is null || ticket.OpenedBy == filter.OpenedBy)
            .Where(ticket => filter.Status is null || ticket.Status == filter.Status)
            .Where(ticket => filter.SectorId is null || ticket.SectorId == filter.SectorId)
            .Where(ticket => filter.LineId is null || ticket.LineId == filter.LineId)
            .Where(ticket => filter.From is null || ticket.OpenedAt >= filter.From)
            .Where(ticket => filter.To is null || ticket.OpenedAt <= filter.To);
}

public sealed class HelpdeskFixture
{
    public HelpdeskFixture()
    {
        Production = new ProductionFixture();
        Access = Production.Access;

        Manager = Access.AddUser(3301, "gestor@example.com", "Gil Gestor", Access.RoleNamed("gestor-helpdesk"));
        Attendant = Access.AddUser(3302, "tecnica@example.com", "Tania Tecnica", Access.RoleNamed("manutencao"));
        Requester = Access.AddUser(3303, "op@example.com", "Otavio Operador", Access.OperatorRole);
        Outsider = Access.AddUser(3304, "fora@example.com", "Fabio Fora", Access.RoleNamed("qualidade"));
        Administrator = Access.AddAdministrator(9101);
    }

    public ProductionFixture Production { get; }

    public AccessFixture Access { get; }

    public FakeHelpdeskSectorRepository Sectors { get; } = new();

    public FakeHelpdeskFieldTemplateRepository Templates { get; } = new();

    public FakeHelpdeskTicketRepository Tickets { get; } = new();

    public User Manager { get; }

    public User Attendant { get; }

    public User Requester { get; }

    public User Outsider { get; }

    public ulong AttendantRoleId => Access.RoleNamed("manutencao").Id;

    public ICurrentActor ManagerActor => FakeCurrentActor.For(Manager);

    public ICurrentActor AttendantActor => FakeCurrentActor.For(Attendant);

    public ICurrentActor RequesterActor => FakeCurrentActor.For(Requester);

    public ICurrentActor OutsiderActor => FakeCurrentActor.For(Outsider);

    public User Administrator { get; }

    public ICurrentActor AdministratorActor => FakeCurrentActor.For(Administrator);

    public CreateHelpdeskSectorUseCase CreateSector(ICurrentActor? actor = null) => new(Sectors, Access.Roles, Access.Users, actor ?? ManagerActor);

    public UpdateHelpdeskSectorUseCase UpdateSector => new(Sectors, Access.Roles);

    public GetHelpdeskSectorUseCase GetSector(ICurrentActor? actor = null) => new(Sectors, Access.Users, actor ?? ManagerActor);

    public ListHelpdeskSectorsUseCase ListSectors(ICurrentActor? actor = null) => new(Sectors, Access.Users, actor ?? ManagerActor);

    public ListHelpdeskFieldTemplatesUseCase ListTemplates => new(Templates);

    public CreateHelpdeskFieldTemplateUseCase CreateTemplate => new(Templates);

    public UpdateHelpdeskFieldTemplateUseCase UpdateTemplate => new(Templates);

    public DeleteHelpdeskFieldTemplateUseCase DeleteTemplate => new(Templates);

    public ListHelpdeskRolesUseCase ListRoles => new(Access.Roles);

    public ListHelpdeskLocationsUseCase Locations => new(Production.Lines, Production.Runs);

    public OpenHelpdeskTicketUseCase Open(ICurrentActor? actor = null) =>
        new(Tickets, Sectors, Production.Lines, Production.Runs, Access.Users, actor ?? RequesterActor);

    public ListHelpdeskTicketsUseCase ListTickets(ICurrentActor? actor = null) =>
        new(Tickets, Sectors, Production.Lines, Access.Users, actor ?? AttendantActor);

    public GetHelpdeskTicketUseCase GetTicket(ICurrentActor? actor = null) =>
        new(Tickets, Sectors, Production.Lines, Access.Users, actor ?? AttendantActor);

    public StartHelpdeskTicketUseCase Start(ICurrentActor? actor = null) => new(Tickets, Sectors, Access.Users, actor ?? AttendantActor);

    public ResolveHelpdeskTicketUseCase Resolve(ICurrentActor? actor = null) => new(Tickets, Sectors, Access.Users, actor ?? AttendantActor);

    public RejectHelpdeskTicketUseCase Reject(ICurrentActor? actor = null) => new(Tickets, Sectors, Access.Users, actor ?? AttendantActor);

    public HelpdeskOeeLossSource LossSource => new(Tickets);

    public static HelpdeskFieldDto Field(
        string id,
        HelpdeskFieldType type,
        string label,
        bool required = true,
        IReadOnlyList<HelpdeskOptionDto>? options = null,
        HelpdeskDependencyDto? dependsOn = null) => new(id, type, label, required, null, options, dependsOn, null);

    public static IReadOnlyList<HelpdeskOptionDto> Options(params string[] values) =>
        [.. values.Select(value => new HelpdeskOptionDto(value, char.ToUpperInvariant(value[0]) + value[1..], null, null))];

    public static HelpdeskFormDto DefaultForm() => new(
    [
        Field("local", HelpdeskFieldType.ProductionSpot, "Onde está o problema?"),
        Field("parada", HelpdeskFieldType.LineStop, "A linha está parada?"),
        Field("tipo", HelpdeskFieldType.CardRadio, "Tipo do problema", true, Options("mecanico", "eletrico")),
        Field("detalhe_mecanico", HelpdeskFieldType.Text, "Qual peça?", true, null, new HelpdeskDependencyDto("tipo", ["mecanico"])),
        Field("defeitos", HelpdeskFieldType.Defects, "Peças com defeito", false),
        Field("descricao", HelpdeskFieldType.TextArea, "Descreva o problema"),
    ]);

    public static HelpdeskFormDto ResolutionForm() => new([Field("solucao", HelpdeskFieldType.TextArea, "O que foi feito?")]);

    public SaveHelpdeskSectorRequest SectorRequest(
        string name = "Manutenção",
        HelpdeskFormDto? form = null,
        HelpdeskFormDto? resolution = null,
        IReadOnlyList<ulong>? roles = null,
        bool active = true,
        string? externalUrl = null,
        IReadOnlyList<string>? summary = null) => new(
        name,
        "Quebras e ajustes",
        "#2547D0",
        "build",
        ["Urgente"],
        active,
        externalUrl,
        form ?? DefaultForm(),
        resolution,
        summary ?? ["tipo", "descricao"],
        roles ?? [AttendantRoleId]);

    public async Task<HelpdeskSector> SectorAsync(SaveHelpdeskSectorRequest? request = null)
    {
        var created = await CreateSector().ExecuteAsync(request ?? SectorRequest(resolution: ResolutionForm()), CancellationToken.None);
        Assert.True(created.IsSuccess, string.Join(";", created.Errors.Select(error => error.Message)));

        return Sectors.Stored.Single(sector => sector.Id == created.Value.Id);
    }

    public async Task<ulong> LineWithRunningPostAsync()
    {
        var run = await Production.StartedRunAsync();
        return run.LineId;
    }

    public OpenHelpdeskTicketRequest TicketRequest(
        ulong sectorId,
        ulong? lineId = null,
        bool stopped = false,
        string tipo = "eletrico",
        string? defects = null,
        ulong? postId = null,
        string? stoppedAt = null)
    {
        var answers = new Dictionary<string, string?>
        {
            ["local"] = (lineId ?? Production.Line.Id).ToString(),
            ["parada"] = stopped ? "stopped" : "running",
            ["tipo"] = tipo,
            ["descricao"] = "Esteira travou",
        };

        if (postId is not null)
            answers["local_post"] = postId.Value.ToString();

        if (stoppedAt is not null)
            answers["parada_time"] = stoppedAt;

        if (tipo == "mecanico")
            answers["detalhe_mecanico"] = "Rolamento";

        if (defects is not null)
            answers["defeitos"] = defects;

        return new(sectorId, answers);
    }

    public async Task<HelpdeskTicket> TicketAsync(HelpdeskSector sector, OpenHelpdeskTicketRequest? request = null)
    {
        var opened = await Open().ExecuteAsync(request ?? TicketRequest(sector.Id), CancellationToken.None);
        Assert.True(opened.IsSuccess, string.Join(";", opened.Errors.Select(error => error.Message)));

        return Tickets.Stored.Single(ticket => ticket.Id == opened.Value.Id);
    }
}
