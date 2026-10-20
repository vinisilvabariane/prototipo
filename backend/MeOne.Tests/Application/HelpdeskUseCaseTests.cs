using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Validators.Helpdesk;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;
using static MeOne.Tests.Fakes.HelpdeskFixture;

namespace MeOne.Tests.Application;

public sealed class HelpdeskUseCaseTests
{
    private static string Message(FluentResults.ResultBase result) => Assert.Single(result.Errors).Message;

    [Fact]
    public async Task Sector_IsCreatedWithItsFormRolesAndSummary()
    {
        var f = new HelpdeskFixture();

        var created = await f.CreateSector().ExecuteAsync(f.SectorRequest(name: " Manutenção ", resolution: ResolutionForm()), CancellationToken.None);

        Assert.True(created.IsSuccess, string.Join(";", created.Errors.Select(error => error.Message)));
        Assert.Equal("Manutenção", created.Value.Name);
        Assert.Equal(6, created.Value.Form.Fields!.Count);
        Assert.Single(created.Value.ResolutionForm.Fields!);
        Assert.Equal(["tipo", "descricao"], created.Value.SummaryFields);
        Assert.Equal([f.AttendantRoleId], created.Value.AttendantRoleIds);
        Assert.True(created.Value.CanManage);
        Assert.False(created.Value.CanAttend);
    }

    [Fact]
    public async Task Sector_RejectsBrokenFormsDuplicatedNamesAndUnknownRoles()
    {
        var f = new HelpdeskFixture();
        await f.SectorAsync();

        var broken = await f.CreateSector().ExecuteAsync(f.SectorRequest(name: "Outro", form: new HelpdeskFormDto([Field("Campo 1", HelpdeskFieldType.Text, "x")])), CancellationToken.None);
        var duplicate = await f.CreateSector().ExecuteAsync(f.SectorRequest(name: "manutenção"), CancellationToken.None);
        var unknownRole = await f.CreateSector().ExecuteAsync(f.SectorRequest(name: "Novo", roles: [9999]), CancellationToken.None);

        Assert.Equal(HelpdeskErrors.FieldIdInvalid, Message(broken));
        Assert.Equal(HelpdeskErrors.NameTaken("manutenção"), Message(duplicate));
        Assert.Equal(HelpdeskErrors.RoleUnknown, Message(unknownRole));
    }

    [Fact]
    public async Task Sector_UpdateChangesTheConfigurationAndChecksNameAndExistence()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var other = await f.SectorAsync(f.SectorRequest(name: "Qualidade", form: new HelpdeskFormDto([Field("nota", HelpdeskFieldType.Text, "Nota")]), summary: []));

        var updated = await f.UpdateSector.ExecuteAsync(sector.Id, f.SectorRequest(name: "Manutenção geral", active: false, summary: []), CancellationToken.None);
        var clash = await f.UpdateSector.ExecuteAsync(sector.Id, f.SectorRequest(name: "qualidade"), CancellationToken.None);
        var missing = await f.UpdateSector.ExecuteAsync(999, f.SectorRequest(), CancellationToken.None);

        Assert.True(updated.IsSuccess);
        Assert.Equal("Manutenção geral", sector.Name);
        Assert.False(sector.Active);
        Assert.Empty(sector.SummaryFields);
        Assert.Equal(HelpdeskErrors.NameTaken("qualidade"), Message(clash));
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
        Assert.NotEqual(sector.Id, other.Id);
    }

    [Fact]
    public async Task Sector_ListHidesInactiveOnesFromEveryoneButManagersAndFlagsAttendance()
    {
        var f = new HelpdeskFixture();
        await f.SectorAsync();
        await f.SectorAsync(f.SectorRequest(name: "Antigo", active: false));

        var asRequester = await f.ListSectors(f.RequesterActor).ExecuteAsync(new HelpdeskSectorsQuery { IncludeInactive = true }, CancellationToken.None);
        var asManager = await f.ListSectors(f.ManagerActor).ExecuteAsync(new HelpdeskSectorsQuery { IncludeInactive = true }, CancellationToken.None);
        var asAttendant = await f.ListSectors(f.AttendantActor).ExecuteAsync(new HelpdeskSectorsQuery(), CancellationToken.None);

        Assert.Equal(["Manutenção"], asRequester.Value.Items.Select(item => item.Name));
        Assert.False(asRequester.Value.CanManage);
        Assert.Equal(2, asManager.Value.Items.Count);
        Assert.True(asManager.Value.CanManage);
        Assert.True(Assert.Single(asAttendant.Value.Items).CanAttend);
        Assert.False(asRequester.Value.Items[0].CanAttend);
    }

    [Fact]
    public async Task Sector_GetReturnsTheDetailOrNotFound()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();

        var found = await f.GetSector(f.AttendantActor).ExecuteAsync(sector.Id, CancellationToken.None);
        var missing = await f.GetSector().ExecuteAsync(999, CancellationToken.None);

        Assert.True(found.Value.CanAttend);
        Assert.False(found.Value.CanManage);
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
    }

    [Fact]
    public async Task FieldTemplates_AreCreatedUpdatedAndDeleted()
    {
        var f = new HelpdeskFixture();
        var request = new SaveHelpdeskFieldTemplateRequest(" Turno ", HelpdeskFieldType.CardRadio, "Qual turno?", true, Options("manha", "tarde"));

        var created = await f.CreateTemplate.ExecuteAsync(request, CancellationToken.None);
        var duplicate = await f.CreateTemplate.ExecuteAsync(request with { Name = "turno" }, CancellationToken.None);
        var invalid = await f.CreateTemplate.ExecuteAsync(request with { Name = "Outro", Type = HelpdeskFieldType.LineStop }, CancellationToken.None);
        var updated = await f.UpdateTemplate.ExecuteAsync(created.Value.Id, request with { Label = "Em qual turno?" }, CancellationToken.None);
        var missing = await f.UpdateTemplate.ExecuteAsync(999, request, CancellationToken.None);
        var list = await f.ListTemplates.ExecuteAsync(CancellationToken.None);
        var deleted = await f.DeleteTemplate.ExecuteAsync(created.Value.Id, CancellationToken.None);
        var deletedAgain = await f.DeleteTemplate.ExecuteAsync(created.Value.Id, CancellationToken.None);

        Assert.Equal("Turno", created.Value.Name);
        Assert.Equal(HelpdeskErrors.NameTaken("turno"), Message(duplicate));
        Assert.Equal(HelpdeskErrors.TemplateTypeInvalid, Message(invalid));
        Assert.True(updated.IsSuccess);
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
        Assert.Equal("Em qual turno?", Assert.Single(list.Value).Label);
        Assert.True(deleted.IsSuccess);
        Assert.IsType<NotFoundError>(Assert.Single(deletedAgain.Errors));
    }

    [Fact]
    public async Task Roles_AreListedForTheAttendantPicker()
    {
        var f = new HelpdeskFixture();

        var roles = await f.ListRoles.ExecuteAsync(CancellationToken.None);

        Assert.Contains(roles.Value, role => role.Name == "gestor-helpdesk");
        Assert.Contains(roles.Value, role => role.Id == f.AttendantRoleId);
    }

    [Fact]
    public async Task Locations_ListActiveLinesWithThePostsOfTheRunningProduction()
    {
        var f = new HelpdeskFixture();
        var idle = MeOne.Domain.Entities.ProductionLine.Create("Linha parada", DateTime.UtcNow);
        f.Production.Lines.Insert(idle);
        await f.LineWithRunningPostAsync();

        var locations = await f.Locations.ExecuteAsync(CancellationToken.None);

        var running = locations.Value.Single(location => location.LineId == f.Production.Line.Id);
        var notRunning = locations.Value.Single(location => location.LineId == idle.Id);
        Assert.Equal(["Montagem", "Maquina A", "Maquina B", "Qualidade"], running.Posts.Select(post => post.Name));
        Assert.Empty(notRunning.Posts);
    }

    [Fact]
    public async Task Open_StoresTheAnswersLocationStopAndDefects()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var run = await f.Production.StartedRunAsync();
        var post = f.Production.PostId(run, "Maquina A");
        var stoppedAt = DateTime.UtcNow.AddMinutes(-15).ToString("O");

        var opened = await f.Open().ExecuteAsync(f.TicketRequest(sector.Id, run.LineId, true, "mecanico", "4", post, stoppedAt), CancellationToken.None);

        Assert.True(opened.IsSuccess, string.Join(";", opened.Errors.Select(error => error.Message)));
        var ticket = f.Tickets.Stored.Single();
        Assert.Equal(HelpdeskTicketStatus.Open, ticket.Status);
        Assert.Equal(f.Requester.Id, ticket.OpenedBy);
        Assert.Equal(run.LineId, ticket.LineId);
        Assert.Equal(post, ticket.PostId);
        Assert.Equal("Maquina A", ticket.PostName);
        Assert.True(ticket.IsLineStopped);
        Assert.Equal(4, ticket.Defects);
        Assert.Equal("Rolamento", ticket.Answers["detalhe_mecanico"]);
        Assert.Equal(6, ticket.FormSnapshot.Fields.Count);
        Assert.Equal("Otavio Operador", opened.Value.OpenedByName);
        Assert.Equal("Linha 1", opened.Value.LineName);
        Assert.Equal(["Tipo do problema", "Descreva o problema"], opened.Value.Summary.Select(item => item.Label));
        Assert.Equal("Mecanico", opened.Value.Summary[0].Display);
        Assert.False(opened.Value.CanStart);
    }

    [Fact]
    public async Task Open_KeepsTheFormSnapshotWhenTheSectorChangesLater()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var ticket = await f.TicketAsync(sector);

        await f.UpdateSector.ExecuteAsync(sector.Id, f.SectorRequest(form: new HelpdeskFormDto([Field("novo", HelpdeskFieldType.Text, "Novo")]), summary: []), CancellationToken.None);
        var detail = await f.GetTicket(f.AttendantActor).ExecuteAsync(ticket.Id, CancellationToken.None);

        Assert.Equal(6, ticket.FormSnapshot.Fields.Count);
        Assert.Contains(detail.Value.Answers, answer => answer.Label == "Descreva o problema");
    }

    [Fact]
    public async Task Open_RejectsInvalidAnswersAndUnavailableSectors()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var inactive = await f.SectorAsync(f.SectorRequest(name: "Inativo", active: false));
        var external = await f.SectorAsync(f.SectorRequest(name: "Externo", externalUrl: "https://exemplo.com"));
        var empty = await f.SectorAsync(f.SectorRequest(name: "Vazio", form: new HelpdeskFormDto([]), summary: []));

        var missingAnswer = await f.Open().ExecuteAsync(new OpenHelpdeskTicketRequest(sector.Id, new Dictionary<string, string?> { ["local"] = "1" }), CancellationToken.None);
        var nullAnswers = await f.Open().ExecuteAsync(new OpenHelpdeskTicketRequest(sector.Id, null), CancellationToken.None);
        var unknownLine = await f.Open().ExecuteAsync(f.TicketRequest(sector.Id, lineId: 999), CancellationToken.None);
        var noRunningPost = await f.Open().ExecuteAsync(f.TicketRequest(sector.Id, postId: 123), CancellationToken.None);
        var inactiveResult = await f.Open().ExecuteAsync(f.TicketRequest(inactive.Id), CancellationToken.None);
        var externalResult = await f.Open().ExecuteAsync(f.TicketRequest(external.Id), CancellationToken.None);
        var emptyResult = await f.Open().ExecuteAsync(f.TicketRequest(empty.Id), CancellationToken.None);
        var unknownSector = await f.Open().ExecuteAsync(f.TicketRequest(999), CancellationToken.None);

        Assert.Contains("Responda", Message(missingAnswer));
        Assert.Contains("Responda", Message(nullAnswers));
        Assert.Equal(HelpdeskErrors.LineNotFound, Message(unknownLine));
        Assert.Equal(HelpdeskErrors.PostNotInLine, Message(noRunningPost));
        Assert.Equal(HelpdeskErrors.SectorInactive, Message(inactiveResult));
        Assert.Equal(HelpdeskErrors.SectorIsExternal, Message(externalResult));
        Assert.Equal(HelpdeskErrors.SectorHasNoForm, Message(emptyResult));
        Assert.IsType<NotFoundError>(Assert.Single(unknownSector.Errors));
        Assert.Empty(f.Tickets.Stored);
    }

    [Fact]
    public async Task Open_RejectsAPostThatBelongsToAnotherLine()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var run = await f.Production.StartedRunAsync();
        var other = MeOne.Domain.Entities.ProductionLine.Create("Linha 2", DateTime.UtcNow);
        f.Production.Lines.Insert(other);

        var result = await f.Open().ExecuteAsync(f.TicketRequest(sector.Id, other.Id, postId: f.Production.PostId(run, "Montagem")), CancellationToken.None);

        Assert.Equal(HelpdeskErrors.PostNotInLine, Message(result));
    }

    [Fact]
    public async Task Board_ShowsAttendantsTheirSectorsAndRequestersTheirOwnTickets()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var quality = await f.SectorAsync(f.SectorRequest(name: "Qualidade", roles: [f.Access.RoleNamed("qualidade").Id]));
        await f.TicketAsync(sector);
        await f.TicketAsync(sector);
        await f.TicketAsync(quality);

        var attendant = await f.ListTickets(f.AttendantActor).ExecuteAsync(new HelpdeskTicketsQuery(), CancellationToken.None);
        var outsider = await f.ListTickets(f.OutsiderActor).ExecuteAsync(new HelpdeskTicketsQuery(), CancellationToken.None);
        var mine = await f.ListTickets(f.RequesterActor).ExecuteAsync(new HelpdeskTicketsQuery { Scope = HelpdeskScope.Mine }, CancellationToken.None);
        var requesterBoard = await f.ListTickets(f.RequesterActor).ExecuteAsync(new HelpdeskTicketsQuery(), CancellationToken.None);
        var administrator = await f.ListTickets(f.AdministratorActor).ExecuteAsync(new HelpdeskTicketsQuery(), CancellationToken.None);

        Assert.Equal(2, attendant.Value.TotalCount);
        Assert.True(attendant.Value.CanAttend);
        Assert.Equal(new HelpdeskStatusCounts(2, 0, 0, 0), attendant.Value.Counts);
        Assert.All(attendant.Value.Items, item => Assert.True(item.CanStart && item.CanReject && !item.CanResolve));
        Assert.Equal(1, outsider.Value.TotalCount);
        Assert.Equal(3, mine.Value.TotalCount);
        Assert.All(mine.Value.Items, item => Assert.False(item.CanStart));
        Assert.Empty(requesterBoard.Value.Items);
        Assert.False(requesterBoard.Value.CanAttend);
        Assert.Equal(3, administrator.Value.TotalCount);
    }

    [Fact]
    public async Task Board_FiltersByStatusSectorLineAndPeriodAndPages()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var first = await f.TicketAsync(sector);
        await f.TicketAsync(sector);
        await f.TicketAsync(sector);
        await f.Start().ExecuteAsync(first.Id, CancellationToken.None);

        var inProgress = await f.ListTickets().ExecuteAsync(new HelpdeskTicketsQuery { Status = HelpdeskTicketStatus.InProgress }, CancellationToken.None);
        var page = await f.ListTickets().ExecuteAsync(new HelpdeskTicketsQuery { PageSize = 2, Page = 1 }, CancellationToken.None);
        var byLine = await f.ListTickets().ExecuteAsync(new HelpdeskTicketsQuery { LineId = 999 }, CancellationToken.None);
        var bySector = await f.ListTickets().ExecuteAsync(new HelpdeskTicketsQuery { SectorId = sector.Id }, CancellationToken.None);
        var past = await f.ListTickets().ExecuteAsync(new HelpdeskTicketsQuery { From = DateTime.UtcNow.AddDays(-9), To = DateTime.UtcNow.AddDays(-8) }, CancellationToken.None);
        var inverted = await f.ListTickets().ExecuteAsync(new HelpdeskTicketsQuery { From = DateTime.UtcNow, To = DateTime.UtcNow.AddDays(-1) }, CancellationToken.None);

        Assert.Equal(1, inProgress.Value.TotalCount);
        Assert.Equal(new HelpdeskStatusCounts(2, 1, 0, 0), inProgress.Value.Counts);
        Assert.Single(page.Value.Items);
        Assert.Equal(3, page.Value.TotalCount);
        Assert.Empty(byLine.Value.Items);
        Assert.Equal(3, bySector.Value.TotalCount);
        Assert.Empty(past.Value.Items);
        Assert.Equal(HelpdeskErrors.PeriodInvalid, Message(inverted));
    }

    [Fact]
    public async Task Detail_IsVisibleToTheRequesterAndAttendantsButNotToOthers()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var ticket = await f.TicketAsync(sector);

        var requester = await f.GetTicket(f.RequesterActor).ExecuteAsync(ticket.Id, CancellationToken.None);
        var attendant = await f.GetTicket(f.AttendantActor).ExecuteAsync(ticket.Id, CancellationToken.None);
        var outsider = await f.GetTicket(f.OutsiderActor).ExecuteAsync(ticket.Id, CancellationToken.None);
        var missing = await f.GetTicket().ExecuteAsync(999, CancellationToken.None);

        Assert.Equal("Otavio Operador", requester.Value.OpenedByName);
        Assert.False(requester.Value.CanStart);
        Assert.True(attendant.Value.CanStart);
        Assert.True(attendant.Value.HasResolutionForm);
        Assert.Contains(attendant.Value.Answers, answer => answer.Type == HelpdeskFieldType.ProductionSpot && answer.Display == "Linha 1");
        Assert.IsType<ForbiddenError>(Assert.Single(outsider.Errors));
        Assert.IsType<NotFoundError>(Assert.Single(missing.Errors));
    }

    [Fact]
    public async Task Attendance_StartThenResolveWithTheResolutionForm()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var ticket = await f.TicketAsync(sector);

        var byOutsider = await f.Start(f.OutsiderActor).ExecuteAsync(ticket.Id, CancellationToken.None);
        var byRequester = await f.Start(f.RequesterActor).ExecuteAsync(ticket.Id, CancellationToken.None);
        var started = await f.Start().ExecuteAsync(ticket.Id, CancellationToken.None);
        var again = await f.Start().ExecuteAsync(ticket.Id, CancellationToken.None);
        var notAssignee = await f.Resolve(f.AdministratorActor).ExecuteAsync(ticket.Id, new ResolveHelpdeskTicketRequest(null, null), CancellationToken.None);
        var withoutAnswer = await f.Resolve().ExecuteAsync(ticket.Id, new ResolveHelpdeskTicketRequest(null, null), CancellationToken.None);
        var resolved = await f.Resolve().ExecuteAsync(
            ticket.Id,
            new ResolveHelpdeskTicketRequest(new Dictionary<string, string?> { ["solucao"] = "Troquei o rolamento" }, " ok "),
            CancellationToken.None);
        var resolvedAgain = await f.Resolve().ExecuteAsync(ticket.Id, new ResolveHelpdeskTicketRequest(null, null), CancellationToken.None);

        Assert.IsType<ForbiddenError>(Assert.Single(byOutsider.Errors));
        Assert.IsType<ForbiddenError>(Assert.Single(byRequester.Errors));
        Assert.True(started.IsSuccess);
        Assert.Equal(f.Attendant.Id, ticket.AssignedTo);
        Assert.Equal(HelpdeskErrors.TicketNotOpen, Message(again));
        Assert.Equal(HelpdeskErrors.OnlyAssigneeResolves, Message(notAssignee));
        Assert.Contains("Responda", Message(withoutAnswer));
        Assert.True(resolved.IsSuccess, string.Join(";", resolved.Errors.Select(error => error.Message)));
        Assert.Equal(HelpdeskTicketStatus.Resolved, ticket.Status);
        Assert.Equal("ok", ticket.Note);
        Assert.Equal("Troquei o rolamento", ticket.ResolutionAnswers["solucao"]);
        Assert.Equal(HelpdeskErrors.TicketNotInProgress, Message(resolvedAgain));

        var detail = await f.GetTicket().ExecuteAsync(ticket.Id, CancellationToken.None);
        Assert.Equal("Tania Tecnica", detail.Value.AssignedToName);
        Assert.Contains(detail.Value.ResolutionAnswers, answer => answer.Display == "Troquei o rolamento");
        Assert.False(detail.Value.CanResolve);
    }

    [Fact]
    public async Task Attendance_SectorWithoutResolutionFormResolvesDirectly()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync(f.SectorRequest());
        var ticket = await f.TicketAsync(sector);

        await f.Start().ExecuteAsync(ticket.Id, CancellationToken.None);
        var resolved = await f.Resolve().ExecuteAsync(ticket.Id, new ResolveHelpdeskTicketRequest(null, "feito"), CancellationToken.None);

        Assert.True(resolved.IsSuccess);
        Assert.Empty(ticket.ResolutionAnswers);
    }

    [Fact]
    public async Task Attendance_RejectOnlyWhileOpenAndByAnAttendant()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var ticket = await f.TicketAsync(sector);
        var started = await f.TicketAsync(sector);
        await f.Start().ExecuteAsync(started.Id, CancellationToken.None);

        var byOutsider = await f.Reject(f.OutsiderActor).ExecuteAsync(ticket.Id, new RejectHelpdeskTicketRequest("duplicado"), CancellationToken.None);
        var rejected = await f.Reject().ExecuteAsync(ticket.Id, new RejectHelpdeskTicketRequest(" duplicado "), CancellationToken.None);
        var late = await f.Reject().ExecuteAsync(started.Id, new RejectHelpdeskTicketRequest("tarde"), CancellationToken.None);
        var detail = await f.GetTicket().ExecuteAsync(ticket.Id, CancellationToken.None);

        Assert.IsType<ForbiddenError>(Assert.Single(byOutsider.Errors));
        Assert.True(rejected.IsSuccess);
        Assert.Equal("duplicado", ticket.RejectReason);
        Assert.Equal(HelpdeskErrors.TicketNotOpen, Message(late));
        Assert.Equal("Tania Tecnica", detail.Value.RejectedByName);
        Assert.Equal("duplicado", detail.Value.RejectReason);
    }

    [Fact]
    public async Task LossSource_TurnsStoppedTicketsIntoDowntimeAndDefects()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var run = await f.Production.StartedRunAsync();
        var post = f.Production.PostId(run, "Maquina A");
        var from = DateTime.UtcNow.AddHours(-2);
        var to = DateTime.UtcNow.AddHours(1);
        var stoppedAt = DateTime.UtcNow.AddMinutes(-30);

        var stopped = await f.TicketAsync(sector, f.TicketRequest(sector.Id, run.LineId, true, defects: "3", postId: post, stoppedAt: stoppedAt.ToString("O")));
        var running = await f.TicketAsync(sector, f.TicketRequest(sector.Id, run.LineId, false, defects: "2"));
        var rejected = await f.TicketAsync(sector, f.TicketRequest(sector.Id, run.LineId, true, stoppedAt: stoppedAt.ToString("O")));
        await f.Reject().ExecuteAsync(rejected.Id, new RejectHelpdeskTicketRequest("duplicado"), CancellationToken.None);

        var open = await f.LossSource.GetAsync(run.LineId, from, to, CancellationToken.None);
        await f.Start().ExecuteAsync(stopped.Id, CancellationToken.None);
        await f.Resolve().ExecuteAsync(stopped.Id, new ResolveHelpdeskTicketRequest(new Dictionary<string, string?> { ["solucao"] = "ok" }, null), CancellationToken.None);
        var resolved = await f.LossSource.GetAsync(run.LineId, from, to, CancellationToken.None);
        var otherLine = await f.LossSource.GetAsync(999, from, to, CancellationToken.None);
        var connection = await f.LossSource.GetAsync(0, from, to, CancellationToken.None);

        var openDowntime = Assert.Single(open.Downtimes);
        Assert.True(open.Available);
        Assert.Equal(post, openDowntime.PostId);
        Assert.Equal(stoppedAt, openDowntime.From, TimeSpan.FromSeconds(1));
        Assert.Equal(to, openDowntime.To);
        Assert.True(openDowntime.AffectsAvailability);
        Assert.False(openDowntime.AffectsPerformance);
        Assert.Equal(OeeDowntimeSource.Ticket, openDowntime.Source);
        Assert.Equal([3, 2], open.Defects.Select(defect => defect.Quantity).Order().Reverse());
        Assert.Equal(stopped.FinishedAt, Assert.Single(resolved.Downtimes).To);
        Assert.Empty(otherLine.Downtimes);
        Assert.True(connection.Available);
        Assert.Empty(connection.Downtimes);
        Assert.Equal(2, running.Defects);
    }

    [Fact]
    public async Task LossSource_FeedsTheOeeCalculator()
    {
        var f = new HelpdeskFixture();
        var sector = await f.SectorAsync();
        var run = await f.Production.StartedRunAsync();
        var started = run.StartedAt!.Value;
        var post = f.Production.PostId(run, "Montagem");

        await f.TicketAsync(sector, f.TicketRequest(sector.Id, run.LineId, true, defects: "10", postId: post, stoppedAt: started.ToString("O")));
        await f.Production.RecordOutput.ExecuteAsync(
            run.Id,
            new MeOne.Application.Communication.Production.Requests.RecordOutputRequest(post, 80, started),
            CancellationToken.None);

        var now = started.AddHours(1);
        var losses = await f.LossSource.GetAsync(run.LineId, started, now, CancellationToken.None);
        var oee = OeeCalculator.Default.Calculate(run, now, losses);
        var montagem = oee.Posts.Single(item => item.PostId == post);

        Assert.True(oee.LossSourceAvailable);
        Assert.Equal(0, montagem.Pillars.Availability, 6);
        Assert.Equal(10, montagem.Variables["defects"]);
        Assert.Equal(70.0 / 80, montagem.Pillars.Quality, 6);
    }

    [Fact]
    public void Validators_CheckTheRequestShapes()
    {
        Assert.False(new SaveHelpdeskSectorRequestValidator().Validate(new SaveHelpdeskSectorRequest(" ", null, null, null, null, true, null, null, null, null, null)).IsValid);
        Assert.True(new SaveHelpdeskSectorRequestValidator().Validate(new HelpdeskFixture().SectorRequest()).IsValid);
        Assert.False(new SaveHelpdeskFieldTemplateRequestValidator().Validate(new SaveHelpdeskFieldTemplateRequest("", HelpdeskFieldType.Text, "x", true, null)).IsValid);
        Assert.False(new SaveHelpdeskFieldTemplateRequestValidator().Validate(new SaveHelpdeskFieldTemplateRequest("m", HelpdeskFieldType.Text, "", true, null)).IsValid);
        Assert.False(new OpenHelpdeskTicketRequestValidator().Validate(new OpenHelpdeskTicketRequest(0, null)).IsValid);
        Assert.False(new RejectHelpdeskTicketRequestValidator().Validate(new RejectHelpdeskTicketRequest(" ")).IsValid);
        Assert.False(new RejectHelpdeskTicketRequestValidator().Validate(new RejectHelpdeskTicketRequest(new string('x', 501))).IsValid);
        Assert.True(new RejectHelpdeskTicketRequestValidator().Validate(new RejectHelpdeskTicketRequest("duplicado")).IsValid);
        Assert.False(new ResolveHelpdeskTicketRequestValidator().Validate(new ResolveHelpdeskTicketRequest(null, new string('x', 1001))).IsValid);

        var query = new HelpdeskTicketsQueryValidator();
        Assert.True(query.Validate(new HelpdeskTicketsQuery()).IsValid);
        Assert.False(query.Validate(new HelpdeskTicketsQuery { Page = -1 }).IsValid);
        Assert.False(query.Validate(new HelpdeskTicketsQuery { PageSize = 101 }).IsValid);
        Assert.False(query.Validate(new HelpdeskTicketsQuery { From = new(2026, 1, 3), To = new(2026, 1, 2) }).IsValid);
        Assert.False(query.Validate(new HelpdeskTicketsQuery { Status = (HelpdeskTicketStatus)9 }).IsValid);
    }
}
