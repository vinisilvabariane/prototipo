using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Domain;

public sealed class HelpdeskDomainTests
{
    private static readonly DateTime Now = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);

    private static HelpdeskOption Opt(string value) => new(value, value.ToUpperInvariant(), null, null);

    private static HelpdeskField F(
        string id,
        HelpdeskFieldType type = HelpdeskFieldType.Text,
        bool required = true,
        IReadOnlyList<HelpdeskOption>? options = null,
        HelpdeskDependency? dependsOn = null,
        string label = "Pergunta") => new(id, type, label, required, null, options ?? [], dependsOn, null);

    private static HelpdeskForm Form(params HelpdeskField[] fields) => new(fields);

    private static HelpdeskForm Branching() => Form(
        F("tipo", HelpdeskFieldType.CardRadio, options: [Opt("a"), Opt("b")]),
        F("detalhe_a", dependsOn: new("tipo", ["a"])),
        F("detalhe_b", dependsOn: new("tipo", ["b"])),
        F("comentario", required: false));

    private static string? Check(HelpdeskForm form, bool resolution = false) => HelpdeskFormRules.Check(form, resolution);

    [Fact]
    public void Form_WithBranchesAndEveryType_IsValid()
    {
        var form = Form(
            F("local", HelpdeskFieldType.ProductionSpot),
            F("parada", HelpdeskFieldType.LineStop),
            F("defeitos", HelpdeskFieldType.Defects, required: false),
            F("tipo", HelpdeskFieldType.Select, options: [Opt("a")]),
            F("peca", dependsOn: new("tipo", ["a"])),
            F("qtd", HelpdeskFieldType.Number),
            F("obs", HelpdeskFieldType.TextArea));

        Assert.Null(Check(form));
        Assert.Null(Check(HelpdeskForm.Empty));
    }

    [Theory]
    [InlineData("Campo 1")]
    [InlineData("1campo")]
    [InlineData("")]
    [InlineData("com-hifen")]
    public void Form_RejectsInvalidFieldIds(string id) => Assert.Equal(HelpdeskErrors.FieldIdInvalid, Check(Form(F(id))));

    [Fact]
    public void Form_RejectsStructuralProblems()
    {
        Assert.Equal(HelpdeskErrors.FieldIdDuplicated, Check(Form(F("a"), F("a"))));
        Assert.Equal(HelpdeskErrors.FieldLabelRequired, Check(Form(F("a", label: " "))));
        Assert.Equal(HelpdeskErrors.OptionsRequired, Check(Form(F("a", HelpdeskFieldType.Select))));
        Assert.Equal(HelpdeskErrors.OptionsNotAllowed, Check(Form(F("a", options: [Opt("x")]))));
        Assert.Equal(HelpdeskErrors.OptionDuplicated, Check(Form(F("a", HelpdeskFieldType.Select, options: [Opt("x"), Opt("x")]))));
        Assert.Equal(HelpdeskErrors.OptionInvalid, Check(Form(F("a", HelpdeskFieldType.Select, options: [new("", "Vazio", null, null)]))));
        Assert.Equal(HelpdeskErrors.FormTooLarge, Check(new HelpdeskForm([.. Enumerable.Range(0, 61).Select(index => F($"c{index}"))])));
    }

    [Fact]
    public void Form_ChecksDependencies()
    {
        var tipo = F("tipo", HelpdeskFieldType.CardRadio, options: [Opt("a")]);

        Assert.Equal(HelpdeskErrors.DependencyInvalid, Check(Form(F("filho", dependsOn: new("tipo", ["a"])), tipo)));
        Assert.Equal(HelpdeskErrors.DependencyInvalid, Check(Form(F("pai"), F("filho", dependsOn: new("pai", ["a"])))));
        Assert.Equal(HelpdeskErrors.DependencyInvalid, Check(Form(F("auto", dependsOn: new("auto", ["a"])))));
        Assert.Equal(HelpdeskErrors.DependencyValueInvalid, Check(Form(tipo, F("filho", dependsOn: new("tipo", ["zzz"])))));
        Assert.Equal(HelpdeskErrors.DependencyValueInvalid, Check(Form(tipo, F("filho", dependsOn: new("tipo", [])))));
    }

    [Fact]
    public void Form_ChecksSpecialFields()
    {
        Assert.Equal(HelpdeskErrors.SpecialDuplicated, Check(Form(F("a", HelpdeskFieldType.ProductionSpot), F("b", HelpdeskFieldType.ProductionSpot))));
        Assert.Equal(HelpdeskErrors.SpecialNeedsLocation, Check(Form(F("a", HelpdeskFieldType.LineStop))));
        Assert.Equal(HelpdeskErrors.SpecialNeedsLocation, Check(Form(F("a", HelpdeskFieldType.Defects))));
        Assert.Equal(HelpdeskErrors.SpecialNotAllowedInResolution, Check(Form(F("a", HelpdeskFieldType.ProductionSpot)), resolution: true));
    }

    [Fact]
    public void Summary_OnlyAcceptsUpToThreeQuestionFields()
    {
        var form = Form(F("a"), F("b"), F("c"), F("d"), F("local", HelpdeskFieldType.ProductionSpot));

        Assert.Null(HelpdeskFormRules.CheckSummary(form, ["a", "b", "c"]));
        Assert.Equal(HelpdeskErrors.SummaryFieldInvalid, HelpdeskFormRules.CheckSummary(form, ["a", "b", "c", "d"]));
        Assert.Equal(HelpdeskErrors.SummaryFieldInvalid, HelpdeskFormRules.CheckSummary(form, ["nope"]));
        Assert.Equal(HelpdeskErrors.SummaryFieldInvalid, HelpdeskFormRules.CheckSummary(form, ["local"]));
        Assert.Equal(HelpdeskErrors.SummaryFieldInvalid, HelpdeskFormRules.CheckSummary(form, ["a", "a"]));
    }

    [Fact]
    public void Visible_FollowsTheChosenBranch()
    {
        var form = Branching();

        var branchA = HelpdeskFormRules.Visible(form, new Dictionary<string, string> { ["tipo"] = "a" }).Select(field => field.Id);
        var none = HelpdeskFormRules.Visible(form, new Dictionary<string, string>()).Select(field => field.Id);

        Assert.Equal(["tipo", "detalhe_a", "comentario"], branchA);
        Assert.Equal(["tipo", "comentario"], none);
    }

    private static HelpdeskAnswerResult Normalize(HelpdeskForm form, params (string Key, string? Value)[] answers) =>
        HelpdeskAnswers.Normalize(form, answers.ToDictionary(pair => pair.Key, pair => pair.Value), Now);

    [Fact]
    public void Answers_KeepOnlyTheVisibleBranchAndEnforceRequiredOnIt()
    {
        var form = Branching();

        var ok = Normalize(form, ("tipo", "a"), ("detalhe_a", " peça "), ("detalhe_b", "ignorada"), ("comentario", " "));
        var missing = Normalize(form, ("tipo", "b"), ("detalhe_a", "fora do ramo"));
        var noChoice = Normalize(form);

        Assert.False(ok.Failed);
        Assert.Equal(new Dictionary<string, string> { ["tipo"] = "a", ["detalhe_a"] = "peça" }, ok.Answers);
        Assert.Equal(HelpdeskErrors.FieldRequired("Pergunta"), missing.Problem);
        Assert.Equal(HelpdeskErrors.FieldRequired("Pergunta"), noChoice.Problem);
    }

    [Fact]
    public void Answers_ValidateEachType()
    {
        var form = Form(
            F("tipo", HelpdeskFieldType.Select, required: false, options: [Opt("a")]),
            F("qtd", HelpdeskFieldType.Number, required: false),
            F("curto", required: false),
            F("longo", HelpdeskFieldType.TextArea, required: false));

        Assert.True(Normalize(form, ("tipo", "zzz")).Failed);
        Assert.True(Normalize(form, ("qtd", "abc")).Failed);
        Assert.False(Normalize(form, ("qtd", "1,5")).Failed);
        Assert.True(Normalize(form, ("curto", new string('x', HelpdeskErrors.MaxTextLength + 1))).Failed);
        Assert.False(Normalize(form, ("longo", new string('x', HelpdeskErrors.MaxTextAreaLength))).Failed);
        Assert.True(Normalize(form, ("longo", new string('x', HelpdeskErrors.MaxTextAreaLength + 1))).Failed);
    }

    private static HelpdeskForm SpotForm() => Form(
        F("local", HelpdeskFieldType.ProductionSpot),
        F("parada", HelpdeskFieldType.LineStop),
        F("defeitos", HelpdeskFieldType.Defects, required: false));

    [Fact]
    public void Answers_ReadTheLocationStopAndDefects()
    {
        var stoppedAt = Now.AddMinutes(-20).ToString("O");
        var result = Normalize(SpotForm(), ("local", "7"), ("local_post", "42"), ("parada", "stopped"), ("parada_time", stoppedAt), ("defeitos", "12"));

        var spot = HelpdeskAnswers.ReadSpot(SpotForm(), result.Answers);

        Assert.False(result.Failed, result.Problem);
        Assert.Equal(7UL, spot.LineId);
        Assert.Equal(42UL, spot.PostId);
        Assert.True(spot.Stopped);
        Assert.Equal(Now.AddMinutes(-20), spot.StoppedAt);
        Assert.Equal(12, spot.Defects);
    }

    [Fact]
    public void Answers_StopWithoutTimeDefaultsToNowAndRunningHasNoStop()
    {
        var stopped = Normalize(SpotForm(), ("local", "7"), ("parada", "stopped"));
        var running = Normalize(SpotForm(), ("local", "7"), ("parada", "running"), ("parada_time", Now.ToString("O")));

        Assert.Equal(Now, HelpdeskAnswers.ReadSpot(SpotForm(), stopped.Answers).StoppedAt);
        Assert.False(HelpdeskAnswers.ReadSpot(SpotForm(), running.Answers).Stopped);
        Assert.DoesNotContain("parada_time", running.Answers.Keys);
    }

    [Fact]
    public void Answers_RejectBadLocationStopAndDefects()
    {
        Assert.True(Normalize(SpotForm(), ("local", "abc"), ("parada", "running")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "0"), ("parada", "running")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("local_post", "x"), ("parada", "running")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "talvez")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "stopped"), ("parada_time", Now.AddHours(1).ToString("O"))).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "stopped"), ("parada_time", Now.AddDays(-8).ToString("O"))).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "stopped"), ("parada_time", "ontem")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "running"), ("defeitos", "-1")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "running"), ("defeitos", "1,5")).Failed);
        Assert.True(Normalize(SpotForm(), ("local", "7"), ("parada", "running"), ("defeitos", "100001")).Failed);
    }

    private static HelpdeskSectorDraft Draft(
        string name = "Manutenção",
        string color = "#2547D0",
        string icon = "build",
        IReadOnlyList<string>? badges = null,
        string? url = null,
        HelpdeskForm? form = null,
        HelpdeskForm? resolution = null,
        IReadOnlyList<string>? summary = null,
        IReadOnlyList<ulong>? roles = null) => new(
        name,
        null,
        color,
        icon,
        badges ?? [],
        true,
        url,
        form ?? Form(F("a")),
        resolution ?? HelpdeskForm.Empty,
        summary ?? [],
        roles ?? [1]);

    [Fact]
    public void Sector_ChecksIdentityFields()
    {
        Assert.Null(HelpdeskSector.Check(Draft()));
        Assert.Equal(HelpdeskErrors.NameRequired, HelpdeskSector.Check(Draft(name: " ")));
        Assert.NotNull(HelpdeskSector.Check(Draft(name: new string('x', 61))));
        Assert.Equal(HelpdeskErrors.ColorInvalid, HelpdeskSector.Check(Draft(color: "azul")));
        Assert.Equal(HelpdeskErrors.IconInvalid, HelpdeskSector.Check(Draft(icon: "rocket")));
        Assert.NotNull(HelpdeskSector.Check(Draft(badges: ["1", "2", "3", "4", "5", "6"])));
        Assert.NotNull(HelpdeskSector.Check(Draft(badges: [" "])));
        Assert.Equal(HelpdeskErrors.UrlInvalid, HelpdeskSector.Check(Draft(url: "ftp://x")));
        Assert.Equal(HelpdeskErrors.UrlInvalid, HelpdeskSector.Check(Draft(url: "nao-e-url")));
        Assert.Null(HelpdeskSector.Check(Draft(url: "https://exemplo.com/chamado")));
        Assert.Equal(HelpdeskErrors.SummaryFieldInvalid, HelpdeskSector.Check(Draft(summary: ["nao-existe"])));
        Assert.Equal(HelpdeskErrors.SpecialNotAllowedInResolution, HelpdeskSector.Check(Draft(resolution: Form(F("local", HelpdeskFieldType.ProductionSpot)))));
    }

    [Fact]
    public void Sector_ExternalLinkDropsTheFormAndRolesDecideWhoAttends()
    {
        var external = HelpdeskSector.Create(Draft(url: " https://exemplo.com ", form: Form(F("a")), summary: ["a"]), Now);
        var internalSector = HelpdeskSector.Create(Draft(color: "#abcdef", roles: [3, 3, 4]), Now);

        Assert.True(external.IsExternal);
        Assert.True(external.Form.IsEmpty);
        Assert.Empty(external.SummaryFields);
        Assert.Equal("https://exemplo.com", external.ExternalUrl);
        Assert.Equal("#ABCDEF", internalSector.ColorHex);
        Assert.Equal([3UL, 4UL], internalSector.AttendantRoleIds);
        Assert.True(internalSector.CanBeAttendedBy([4], false));
        Assert.False(internalSector.CanBeAttendedBy([9], false));
        Assert.True(internalSector.CanBeAttendedBy([], true));
    }

    [Fact]
    public void FieldTemplate_OnlyAcceptsSimpleTypesAndValidOptions()
    {
        HelpdeskFieldTemplateDraft Template(HelpdeskFieldType type, params HelpdeskOption[] options) => new("Modelo", type, "Título", true, options);

        Assert.Null(HelpdeskFieldTemplate.Check(Template(HelpdeskFieldType.Text)));
        Assert.Null(HelpdeskFieldTemplate.Check(Template(HelpdeskFieldType.CardRadio, Opt("a"))));
        Assert.Equal(HelpdeskErrors.TemplateTypeInvalid, HelpdeskFieldTemplate.Check(Template(HelpdeskFieldType.LineStop)));
        Assert.Equal(HelpdeskErrors.OptionsRequired, HelpdeskFieldTemplate.Check(Template(HelpdeskFieldType.Select)));
        Assert.Equal(HelpdeskErrors.NameRequired, HelpdeskFieldTemplate.Check(Template(HelpdeskFieldType.Text) with { Name = " " }));
    }

    private static HelpdeskTicket OpenTicket(bool stopped = false, int defects = 0)
    {
        var sector = HelpdeskSector.Create(Draft(), Now);
        sector.Id = 5;

        return HelpdeskTicket.Open(
            sector,
            10,
            new Dictionary<string, string> { ["a"] = "x" },
            new HelpdeskSpot(7, 42, stopped, stopped ? Now.AddMinutes(-30) : null, defects),
            "Solda A",
            Now);
    }

    [Fact]
    public void Ticket_FollowsOpenStartResolve()
    {
        var ticket = OpenTicket();

        Assert.Null(ticket.CheckStart());
        Assert.Equal(HelpdeskErrors.TicketNotInProgress, ticket.CheckResolve(20));

        ticket.Start(20, Now.AddMinutes(5));

        Assert.Equal(HelpdeskTicketStatus.InProgress, ticket.Status);
        Assert.Equal(HelpdeskErrors.TicketNotOpen, ticket.CheckStart());
        Assert.Equal(HelpdeskErrors.TicketNotOpen, ticket.CheckReject());
        Assert.Equal(HelpdeskErrors.OnlyAssigneeResolves, ticket.CheckResolve(21));
        Assert.Null(ticket.CheckResolve(20));

        ticket.Resolve(HelpdeskForm.Empty, new Dictionary<string, string>(), "  feito  ", Now.AddHours(1));

        Assert.Equal(HelpdeskTicketStatus.Resolved, ticket.Status);
        Assert.Equal("feito", ticket.Note);
        Assert.Equal(Now.AddHours(1), ticket.FinishedAt);
        Assert.True(ticket.IsClosed);
        Assert.Equal(HelpdeskErrors.TicketNotInProgress, ticket.CheckResolve(20));
    }

    [Fact]
    public void Ticket_CanBeRejectedOnlyWhileOpen()
    {
        var ticket = OpenTicket(stopped: true);

        ticket.Reject(20, "  duplicado  ", Now.AddMinutes(2));

        Assert.Equal(HelpdeskTicketStatus.Rejected, ticket.Status);
        Assert.Equal("duplicado", ticket.RejectReason);
        Assert.Equal(20UL, ticket.RejectedBy);
        Assert.Null(ticket.DowntimeStart);
        Assert.Equal(HelpdeskErrors.TicketNotOpen, ticket.CheckStart());
    }

    [Fact]
    public void Ticket_DowntimeRunsFromTheStopUntilItIsResolvedOrNow()
    {
        var stopped = OpenTicket(stopped: true);
        var running = OpenTicket();

        Assert.Equal(Now.AddMinutes(-30), stopped.DowntimeStart);
        Assert.Equal(Now.AddHours(2), stopped.DowntimeEnd(Now.AddHours(2)));
        Assert.Null(running.DowntimeStart);

        stopped.Start(20, Now);
        stopped.Resolve(HelpdeskForm.Empty, new Dictionary<string, string>(), null, Now.AddMinutes(10));

        Assert.Equal(Now.AddMinutes(10), stopped.DowntimeEnd(Now.AddHours(5)));
        Assert.Equal(7UL, stopped.LineId);
        Assert.Equal("Solda A", stopped.PostName);
    }
}
