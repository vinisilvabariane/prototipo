using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using MeOne.API.Controllers;
using MeOne.API.Errors;
using MeOne.API.Validation;
using MeOne.Application.Interfaces;
using MeOne.Application.Validators.Auth;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure;
using MeOne.Tests.Fakes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeOne.Tests.API;

public sealed class HelpdeskEndpointTests : IAsyncLifetime
{
    private readonly HelpdeskFixture _fixture = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services
            .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddApplicationPart(typeof(HelpdeskTicketsController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(ErrorResponse.From(
                FluentValidationFilter.Title,
                "Corpo da requisição inválido ou mal formatado.")));

        builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        builder.Services.AddApplicationUseCases();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<IHelpdeskSectorRepository>(_fixture.Sectors);
        builder.Services.AddSingleton<IHelpdeskFieldTemplateRepository>(_fixture.Templates);
        builder.Services.AddSingleton<IHelpdeskTicketRepository>(_fixture.Tickets);
        builder.Services.AddSingleton<IProductionLineRepository>(_fixture.Production.Lines);
        builder.Services.AddSingleton<IProductionRunRepository>(_fixture.Production.Runs);
        builder.Services.AddSingleton<IRoleRepository>(_fixture.Access.Roles);
        builder.Services.AddSingleton<IUserRepository>(_fixture.Access.Users);
        builder.Services.AddScoped<ICurrentActor>(_ => _fixture.AdministratorActor);

        builder.Services.AddSingleton<IAuthorizationPolicyProvider, AllowEverythingPolicyProvider>();
        builder.Services.AddAuthorization();

        _app = builder.Build();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();

        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.First()) };
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    private static object SectorBody(string name = "Manutenção") => new
    {
        name,
        description = "Quebras",
        colorHex = "#2547D0",
        icon = "build",
        badges = new[] { "Urgente" },
        active = true,
        externalUrl = (string?)null,
        form = new
        {
            fields = new object[]
            {
                new { id = "local", type = "productionSpot", label = "Onde?", required = true },
                new { id = "parada", type = "lineStop", label = "Linha parada?", required = true },
                new
                {
                    id = "tipo",
                    type = "cardRadio",
                    label = "Tipo",
                    required = true,
                    options = new[] { new { value = "mecanico", label = "Mecânico" }, new { value = "eletrico", label = "Elétrico" } },
                },
                new { id = "peca", type = "text", label = "Qual peça?", required = true, dependsOn = new { fieldId = "tipo", values = new[] { "mecanico" } } },
            },
        },
        resolutionForm = new { fields = new object[] { new { id = "solucao", type = "textArea", label = "O que foi feito?", required = true } } },
        summaryFields = new[] { "tipo" },
        attendantRoleIds = Array.Empty<ulong>(),
    };

    private async Task<ulong> CreateSectorAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/helpdesk/sectors", SectorBody());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetUInt64();
    }

    [Fact]
    public async Task Sector_IsCreatedReadBackAndListed()
    {
        var id = await CreateSectorAsync();

        var detail = await _client.GetAsync($"/api/v1/helpdesk/sectors/{id}");
        var list = await _client.GetAsync("/api/v1/helpdesk/sectors");

        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using var detailBody = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        var fields = detailBody.RootElement.GetProperty("form").GetProperty("fields");
        Assert.Equal(4, fields.GetArrayLength());
        Assert.Equal("cardRadio", fields[2].GetProperty("type").GetString());
        Assert.Equal("tipo", fields[3].GetProperty("dependsOn").GetProperty("fieldId").GetString());
        Assert.True(detailBody.RootElement.GetProperty("canManage").GetBoolean());
        using var listBody = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(1, listBody.RootElement.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task Sector_WithABrokenFormIs400WithTheMessage()
    {
        var body = new
        {
            name = "Quebrado",
            colorHex = "#2547D0",
            icon = "build",
            active = true,
            form = new { fields = new object[] { new { id = "Campo 1", type = "text", label = "x", required = true } } },
        };

        var response = await _client.PostAsJsonAsync("/api/v1/helpdesk/sectors", body);
        var unknownType = await _client.PostAsJsonAsync("/api/v1/helpdesk/sectors", new { name = "X", colorHex = "#2547D0", icon = "build", active = true, form = new { fields = new object[] { new { id = "a", type = "wizard", label = "x" } } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("identificador", json.RootElement.GetProperty("details").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, unknownType.StatusCode);
    }

    [Fact]
    public async Task Sector_UnknownIs404()
    {
        var response = await _client.GetAsync("/api/v1/helpdesk/sectors/4040");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FieldTemplates_AreCreatedListedAndDeleted()
    {
        var created = await _client.PostAsJsonAsync("/api/v1/helpdesk/field-templates", new
        {
            name = "Turno",
            type = "select",
            label = "Qual turno?",
            required = true,
            options = new[] { new { value = "a", label = "A" } },
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdBody.RootElement.GetProperty("id").GetUInt64();

        var list = await _client.GetAsync("/api/v1/helpdesk/field-templates");
        var deleted = await _client.DeleteAsync($"/api/v1/helpdesk/field-templates/{id}");

        using var listBody = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(1, listBody.RootElement.GetArrayLength());
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Roles_AndLocations_AreExposed()
    {
        await _fixture.Production.StartedRunAsync();

        var roles = await _client.GetAsync("/api/v1/helpdesk/roles");
        var locations = await _client.GetAsync("/api/v1/helpdesk/locations");

        using var rolesBody = JsonDocument.Parse(await roles.Content.ReadAsStringAsync());
        using var locationsBody = JsonDocument.Parse(await locations.Content.ReadAsStringAsync());
        Assert.True(rolesBody.RootElement.GetArrayLength() > 5);
        Assert.Equal("Linha 1", locationsBody.RootElement[0].GetProperty("lineName").GetString());
        Assert.Equal(4, locationsBody.RootElement[0].GetProperty("posts").GetArrayLength());
    }

    [Fact]
    public async Task Ticket_IsOpenedAttendedAndResolvedThroughTheApi()
    {
        var sectorId = await CreateSectorAsync();
        var lineId = _fixture.Production.Line.Id;

        var opened = await _client.PostAsJsonAsync("/api/v1/helpdesk/tickets", new
        {
            sectorId,
            answers = new Dictionary<string, string>
            {
                ["local"] = lineId.ToString(),
                ["parada"] = "stopped",
                ["tipo"] = "mecanico",
                ["peca"] = "Rolamento",
            },
        });
        Assert.Equal(HttpStatusCode.Created, opened.StatusCode);
        using var openedBody = JsonDocument.Parse(await opened.Content.ReadAsStringAsync());
        var id = openedBody.RootElement.GetProperty("id").GetUInt64();
        Assert.Equal("open", openedBody.RootElement.GetProperty("status").GetString());
        Assert.True(openedBody.RootElement.GetProperty("isLineStopped").GetBoolean());

        var board = await _client.GetAsync("/api/v1/helpdesk/tickets?status=open");
        using var boardBody = JsonDocument.Parse(await board.Content.ReadAsStringAsync());
        Assert.Equal(1, boardBody.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, boardBody.RootElement.GetProperty("counts").GetProperty("open").GetInt32());

        var started = await _client.PostAsync($"/api/v1/helpdesk/tickets/{id}/start", null);
        var withoutAnswer = await _client.PostAsJsonAsync($"/api/v1/helpdesk/tickets/{id}/resolve", new { answers = new Dictionary<string, string>(), note = "x" });
        var resolved = await _client.PostAsJsonAsync($"/api/v1/helpdesk/tickets/{id}/resolve", new { answers = new Dictionary<string, string> { ["solucao"] = "Troca" }, note = "ok" });
        var detail = await _client.GetAsync($"/api/v1/helpdesk/tickets/{id}");

        Assert.Equal(HttpStatusCode.NoContent, started.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, withoutAnswer.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, resolved.StatusCode);
        using var detailBody = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Equal("resolved", detailBody.RootElement.GetProperty("status").GetString());
        Assert.True(detailBody.RootElement.GetProperty("downtimeSeconds").GetInt32() >= 0);
        Assert.Equal("Troca", detailBody.RootElement.GetProperty("resolutionAnswers")[0].GetProperty("display").GetString());
    }

    [Fact]
    public async Task Ticket_RejectNeedsAReasonAndUnknownIs404()
    {
        var sectorId = await CreateSectorAsync();
        var opened = await _client.PostAsJsonAsync("/api/v1/helpdesk/tickets", new
        {
            sectorId,
            answers = new Dictionary<string, string>
            {
                ["local"] = _fixture.Production.Line.Id.ToString(),
                ["parada"] = "running",
                ["tipo"] = "eletrico",
            },
        });
        using var openedBody = JsonDocument.Parse(await opened.Content.ReadAsStringAsync());
        var id = openedBody.RootElement.GetProperty("id").GetUInt64();

        var noReason = await _client.PostAsJsonAsync($"/api/v1/helpdesk/tickets/{id}/reject", new { reason = " " });
        var rejected = await _client.PostAsJsonAsync($"/api/v1/helpdesk/tickets/{id}/reject", new { reason = "duplicado" });
        var missing = await _client.GetAsync("/api/v1/helpdesk/tickets/4040");

        Assert.Equal(HttpStatusCode.BadRequest, noReason.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, rejected.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Tickets_WithAnInvertedPeriod_Is400()
    {
        var response = await _client.GetAsync("/api/v1/helpdesk/tickets?from=2026-03-10&to=2026-03-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed class AllowEverythingPolicyProvider : IAuthorizationPolicyProvider
    {
        private static readonly AuthorizationPolicy Policy = new AuthorizationPolicyBuilder()
            .RequireAssertion(_ => true)
            .Build();

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => Task.FromResult(Policy);

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) => Task.FromResult<AuthorizationPolicy?>(Policy);
    }
}
