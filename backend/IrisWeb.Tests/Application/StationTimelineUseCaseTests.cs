using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.UseCases.Stations;
using IrisWeb.Application.Validators.Stations;
using IrisWeb.Domain.Entities;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class StationTimelineUseCaseTests
{
    private static readonly DateTime Today = DateTime.Today;

    private static StationPresence Presence(
        long id,
        string registration,
        DateTime enteredAt,
        DateTime? leftAt,
        string workstation = "POSTO 1",
        string line = "LINHA 3") =>
        StationPresence.Restore(
            id,
            registration,
            workstation,
            line,
            enteredAt,
            leftAt,
            leftAt is null ? null : (int)Math.Round((leftAt.Value - enteredAt).TotalSeconds));

    private static StationVacancy Vacancy(
        long id,
        DateTime startedAt,
        DateTime? endedAt,
        string workstation = "POSTO 1",
        string line = "LINHA 3") =>
        StationVacancy.Restore(
            id,
            workstation,
            line,
            startedAt,
            endedAt,
            endedAt is null ? null : (int)Math.Round((endedAt.Value - startedAt).TotalSeconds));

    private static ListStationTimelineUseCase Build(
        FakeStationPresenceRepository presences,
        FakeStationVacancyRepository vacancies,
        FakeStationUnknownRepository? unknowns = null) => new(presences, vacancies, unknowns ?? new FakeStationUnknownRepository());

    [Fact]
    public async Task Desconhecido_entra_na_timeline_com_diagnostico_e_soma_separado()
    {
        var presences = new FakeStationPresenceRepository();
        var vacancies = new FakeStationVacancyRepository();
        var unknowns = new FakeStationUnknownRepository();

        presences.Seed(Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)));
        vacancies.Seed(Vacancy(2, Today.AddHours(9), Today.AddHours(9).AddMinutes(10)));
        unknowns.Seed(StationUnknown.Restore(3, "POSTO 1", "LINHA 3", Today.AddHours(10), Today.AddHours(10).AddMinutes(20), 1200, "16271", 0.91f));

        var result = await Build(presences, vacancies, unknowns).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1) },
            CancellationToken.None);

        var day = Assert.Single(result.Value.Days);
        Assert.Equal(3600, day.OccupiedSeconds);
        Assert.Equal(600, day.VacantSeconds);
        Assert.Equal(1200, day.UnknownSeconds);
        Assert.Equal(1200, result.Value.UnknownSeconds);
        Assert.Equal(1, day.PeopleCount);

        var unknown = Assert.Single(day.Entries, entry => entry.Kind == "unknown");
        Assert.Null(unknown.Registration);
        Assert.Equal("16271", unknown.CandidateRegistration);
        Assert.Equal(0.91f, unknown.CandidateDistance);
    }

    [Fact]
    public async Task Filtro_por_matricula_nao_traz_desconhecido()
    {
        var unknowns = new FakeStationUnknownRepository();
        unknowns.Seed(StationUnknown.Restore(3, "POSTO 1", "LINHA 3", Today.AddHours(10), Today.AddHours(11), 3600, null, null));

        var result = await Build(new FakeStationPresenceRepository(), new FakeStationVacancyRepository(), unknowns).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1), Registration = "1234" },
            CancellationToken.None);

        Assert.Empty(result.Value.Days);
    }

    [Fact]
    public async Task Consulta_agrupa_por_dia_do_mais_recente_para_o_mais_antigo()
    {
        var presences = new FakeStationPresenceRepository();
        var vacancies = new FakeStationVacancyRepository();

        presences.Seed(
            Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)),
            Presence(2, "5678", Today.AddDays(-1).AddHours(8), Today.AddDays(-1).AddHours(10)));

        var result = await Build(presences, vacancies).ExecuteAsync(
            new StationTimelineQuery { From = Today.AddDays(-3), To = Today.AddDays(1) },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var days = result.Value.Days;

        Assert.Equal(2, days.Count);
        Assert.Equal(DateOnly.FromDateTime(Today), days[0].Date);
        Assert.Equal(DateOnly.FromDateTime(Today.AddDays(-1)), days[1].Date);
    }

    [Fact]
    public async Task Cada_dia_soma_ocupado_e_vazio_separadamente()
    {
        var presences = new FakeStationPresenceRepository();
        var vacancies = new FakeStationVacancyRepository();

        presences.Seed(Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)));
        vacancies.Seed(Vacancy(1, Today.AddHours(9), Today.AddHours(9).AddMinutes(30)));

        var result = await Build(presences, vacancies).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1) },
            CancellationToken.None);

        var day = Assert.Single(result.Value.Days);

        Assert.Equal(3600, day.OccupiedSeconds);
        Assert.Equal(1800, day.VacantSeconds);
        Assert.Equal(2, day.Entries.Count);
    }

    [Fact]
    public async Task Pessoas_do_dia_contam_matriculas_distintas_nao_sessoes()
    {
        var presences = new FakeStationPresenceRepository();

        presences.Seed(
            Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)),
            Presence(2, "1234", Today.AddHours(10), Today.AddHours(11)),
            Presence(3, "5678", Today.AddHours(12), Today.AddHours(13)));

        var result = await Build(presences, new FakeStationVacancyRepository()).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1) },
            CancellationToken.None);

        var day = Assert.Single(result.Value.Days);

        Assert.Equal(3, day.Entries.Count);
        Assert.Equal(2, day.PeopleCount);
    }

    [Fact]
    public async Task Sessao_aberta_conta_o_tempo_ate_agora_e_vem_marcada()
    {
        var presences = new FakeStationPresenceRepository();
        presences.Seed(Presence(1, "1234", DateTime.Now.AddMinutes(-10), null));

        var result = await Build(presences, new FakeStationVacancyRepository()).ExecuteAsync(
            new StationTimelineQuery(),
            CancellationToken.None);

        var entry = Assert.Single(result.Value.Days[0].Entries);

        Assert.True(entry.Open);
        Assert.Null(entry.EndedAt);
        Assert.InRange(entry.DurationSeconds, 590, 610);
    }

    [Fact]
    public async Task Filtrar_por_matricula_esconde_os_periodos_de_posto_vazio()
    {
        var presences = new FakeStationPresenceRepository();
        var vacancies = new FakeStationVacancyRepository();

        presences.Seed(Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)));
        vacancies.Seed(Vacancy(1, Today.AddHours(9), Today.AddHours(10)));

        var result = await Build(presences, vacancies).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1), Registration = "1234" },
            CancellationToken.None);

        var day = Assert.Single(result.Value.Days);
        var entry = Assert.Single(day.Entries);

        Assert.Equal("presence", entry.Kind);
        Assert.Equal(0, day.VacantSeconds);
    }

    [Fact]
    public async Task Filtrar_por_posto_ignora_o_que_aconteceu_em_outro()
    {
        var presences = new FakeStationPresenceRepository();

        presences.Seed(
            Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)),
            Presence(2, "5678", Today.AddHours(8), Today.AddHours(9), workstation: "POSTO 2"));

        var result = await Build(presences, new FakeStationVacancyRepository()).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1), Workstation = "POSTO 2" },
            CancellationToken.None);

        var entry = Assert.Single(result.Value.Days[0].Entries);

        Assert.Equal("POSTO 2", entry.Workstation);
    }

    [Fact]
    public async Task Sem_periodo_informado_a_consulta_cobre_o_dia_de_hoje()
    {
        var presences = new FakeStationPresenceRepository();

        presences.Seed(
            Presence(1, "1234", DateTime.Now.AddMinutes(-30), DateTime.Now.AddMinutes(-20)),
            Presence(2, "5678", Today.AddDays(-2).AddHours(8), Today.AddDays(-2).AddHours(9)));

        var result = await Build(presences, new FakeStationVacancyRepository()).ExecuteAsync(
            new StationTimelineQuery(),
            CancellationToken.None);

        Assert.Equal(1, result.Value.EntryCount);
        Assert.Equal(DateOnly.FromDateTime(Today), Assert.Single(result.Value.Days).Date);
    }

    [Fact]
    public async Task Periodo_maior_que_o_teto_e_cortado_no_teto()
    {
        var result = await Build(new FakeStationPresenceRepository(), new FakeStationVacancyRepository())
            .ExecuteAsync(
                new StationTimelineQuery { From = Today.AddDays(-365), To = Today },
                CancellationToken.None);

        var window = result.Value.To - result.Value.From;

        Assert.Equal(ListStationTimelineUseCase.MaxRangeDays, window.TotalDays);
    }

    [Fact]
    public async Task Pagina_corta_as_linhas_mas_nao_os_totais_do_dia()
    {
        var presences = new FakeStationPresenceRepository();

        presences.Seed([.. Enumerable.Range(0, 5).Select(i =>
            Presence(i + 1, $"100{i}", Today.AddHours(8 + i), Today.AddHours(9 + i)))]);

        var useCase = Build(presences, new FakeStationVacancyRepository());

        var first = await useCase.ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1), Page = 1, PageSize = 2 },
            CancellationToken.None);

        var day = Assert.Single(first.Value.Days);

        Assert.Equal(2, day.Entries.Count);
        Assert.Equal(5, day.EntryCount);
        Assert.Equal(5 * 3600, day.OccupiedSeconds);
        Assert.Equal(5, day.PeopleCount);
        Assert.Equal(5, first.Value.EntryCount);
        Assert.Equal(3, first.Value.TotalPages);
    }

    [Fact]
    public async Task Totais_do_periodo_ignoram_a_pagina()
    {
        var presences = new FakeStationPresenceRepository();
        var vacancies = new FakeStationVacancyRepository();

        presences.Seed(
            Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)),
            Presence(2, "5678", Today.AddDays(-1).AddHours(8), Today.AddDays(-1).AddHours(9)));
        vacancies.Seed(Vacancy(1, Today.AddHours(9), Today.AddHours(10)));

        var result = await Build(presences, vacancies).ExecuteAsync(
            new StationTimelineQuery
            {
                From = Today.AddDays(-2),
                To = Today.AddDays(1),
                Page = 1,
                PageSize = 1,
            },
            CancellationToken.None);

        Assert.Single(result.Value.Days);
        Assert.Equal(2, result.Value.DayCount);
        Assert.Equal(2 * 3600, result.Value.OccupiedSeconds);
        Assert.Equal(3600, result.Value.VacantSeconds);
        Assert.Equal(2, result.Value.PeopleCount);
    }

    [Fact]
    public async Task Paginas_seguintes_continuam_de_onde_a_anterior_parou_sem_repetir()
    {
        var presences = new FakeStationPresenceRepository();

        presences.Seed([.. Enumerable.Range(0, 5).Select(i =>
            Presence(i + 1, $"100{i}", Today.AddHours(8 + i), Today.AddHours(9 + i)))]);

        var useCase = Build(presences, new FakeStationVacancyRepository());

        var seen = new List<long>();

        for (var page = 1; page <= 3; page++)
        {
            var result = await useCase.ExecuteAsync(
                new StationTimelineQuery { From = Today, To = Today.AddDays(1), Page = page, PageSize = 2 },
                CancellationToken.None);

            seen.AddRange(result.Value.Days.SelectMany(day => day.Entries).Select(entry => entry.Id));
        }

        Assert.Equal(5, seen.Count);
        Assert.Equal(5, seen.Distinct().Count());
    }

    [Fact]
    public async Task Pagina_alem_do_fim_volta_para_a_ultima_em_vez_de_vir_vazia()
    {
        var presences = new FakeStationPresenceRepository();
        presences.Seed(Presence(1, "1234", Today.AddHours(8), Today.AddHours(9)));

        var result = await Build(presences, new FakeStationVacancyRepository()).ExecuteAsync(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1), Page = 99, PageSize = 10 },
            CancellationToken.None);

        Assert.Equal(1, result.Value.Page);
        Assert.Single(result.Value.Days[0].Entries);
    }

    [Fact]
    public async Task Sem_paginacao_informada_usa_o_tamanho_padrao()
    {
        var result = await Build(new FakeStationPresenceRepository(), new FakeStationVacancyRepository())
            .ExecuteAsync(new StationTimelineQuery(), CancellationToken.None);

        Assert.Equal(ListStationTimelineUseCase.DefaultPageSize, result.Value.PageSize);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(0, result.Value.TotalPages);
    }

    [Fact]
    public void Validador_recusa_pagina_zero_e_tamanho_acima_do_teto()
    {
        var validator = new StationTimelineQueryValidator();

        Assert.False(validator.Validate(new StationTimelineQuery { Page = 0 }).IsValid);
        Assert.False(validator.Validate(
            new StationTimelineQuery { PageSize = ListStationTimelineUseCase.MaxPageSize + 1 }).IsValid);
        Assert.True(validator.Validate(new StationTimelineQuery { Page = 3, PageSize = 50 }).IsValid);
    }

    [Fact]
    public void Validador_recusa_periodo_invertido_e_matricula_com_letra()
    {
        var validator = new StationTimelineQueryValidator();

        Assert.False(validator.Validate(
            new StationTimelineQuery { From = Today, To = Today.AddDays(-1) }).IsValid);

        Assert.False(validator.Validate(
            new StationTimelineQuery { Registration = "12A4" }).IsValid);

        Assert.True(validator.Validate(
            new StationTimelineQuery { From = Today, To = Today.AddDays(1), Registration = "1234" }).IsValid);
    }
}
