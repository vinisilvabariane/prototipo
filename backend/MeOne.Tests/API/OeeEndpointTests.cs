using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using MeOne.API.Controllers;
using MeOne.API.Errors;
using MeOne.API.Validation;
using MeOne.Application.Communication.Production.Requests;
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

public sealed class OeeEndpointTests : IAsyncLifetime
{
    private readonly ProductionFixture _fixture = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services
            .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddApplicationPart(typeof(OeeController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(ErrorResponse.From(
                FluentValidationFilter.Title,
                "Corpo da requisição inválido ou mal formatado.")));

        builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        builder.Services.AddApplicationUseCases();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<IProductionLineRepository>(_fixture.Lines);
        builder.Services.AddSingleton<IProductionShiftRepository>(_fixture.Shifts);
        builder.Services.AddSingleton<IProductionProductRepository>(_fixture.Products);
        builder.Services.AddSingleton<ILineLayoutRepository>(_fixture.Layouts);
        builder.Services.AddSingleton<IProductionRunRepository>(_fixture.Runs);
        builder.Services.AddSingleton<IOeeFormulaSetRepository>(_fixture.FormulaSets);
        builder.Services.AddSingleton<IOeeLossSource>(_fixture.LossSource);
        builder.Services.AddSingleton<IUserRepository>(_fixture.Access.Users);
        builder.Services.AddScoped<ICurrentActor>(_ => _fixture.FullAccessActor);

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

    private static object Formulas(string performance = "real_production / theoretical_production") => new
    {
        availability = "(elapsed_seconds - downtime_availability) / elapsed_seconds",
        performance,
        quality = "if(real_production > 0, (real_production - defects) / real_production, 1)",
        machineOee = "availability * performance * quality",
        lineAvailability = "availability_average",
        linePerformance = "performance_last_post",
        lineQuality = "quality_product",
        lineOee = "availability * performance * quality",
    };

    [Fact]
    public async Task Config_ListsTheCatalogAndNoSetsAtFirst()
    {
        var response = await _client.GetAsync("/api/v1/oee/config");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, body.RootElement.GetProperty("sets").GetArrayLength());
        Assert.Equal(8, body.RootElement.GetProperty("slots").GetArrayLength());
        Assert.True(body.RootElement.GetProperty("variables").GetArrayLength() > 40);
        Assert.True(body.RootElement.GetProperty("canManage").GetBoolean());
        Assert.False(body.RootElement.GetProperty("lossSourceAvailable").GetBoolean());
        Assert.Equal("real_production / theoretical_production", body.RootElement.GetProperty("default").GetProperty("performance").GetString());
    }

    [Fact]
    public async Task FormulaSet_IsCreatedActivatedTestedAndDeleted()
    {
        var created = await _client.PostAsJsonAsync("/api/v1/oee/formulas", new { name = "Meta fixa", formulas = Formulas("real_production / 160"), activate = false });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdBody.RootElement.GetProperty("id").GetUInt64();

        var activate = await _client.PutAsJsonAsync("/api/v1/oee/formulas/active", new { setId = id });
        Assert.Equal(HttpStatusCode.NoContent, activate.StatusCode);

        var blocked = await _client.DeleteAsync($"/api/v1/oee/formulas/{id}");
        Assert.Equal(HttpStatusCode.BadRequest, blocked.StatusCode);

        var test = await _client.PostAsJsonAsync("/api/v1/oee/formulas/test", new
        {
            slot = "performance",
            expression = "real_production / theoretical_production",
            values = new Dictionary<string, double> { ["real_production"] = 30, ["theoretical_production"] = 60 },
        });
        using var testBody = JsonDocument.Parse(await test.Content.ReadAsStringAsync());
        Assert.True(testBody.RootElement.GetProperty("valid").GetBoolean());
        Assert.Equal(0.5, testBody.RootElement.GetProperty("result").GetDouble());

        var reset = await _client.PutAsJsonAsync("/api/v1/oee/formulas/active", new { setId = (ulong?)null });
        var deleted = await _client.DeleteAsync($"/api/v1/oee/formulas/{id}");

        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task FormulaSet_WithABrokenFormula_Is400WithTheMessage()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/oee/formulas", new { name = "Quebrada", formulas = Formulas("real_production /"), activate = false });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("desempenho do posto", body.RootElement.GetProperty("details").GetString());
    }

    [Fact]
    public async Task FormulaTest_WithAnUnknownSlot_Is400()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/oee/formulas/test", new { slot = "x", expression = "1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Runs_AndRunDetail_ExposeThePostsAndTheLine()
    {
        var run = await _fixture.StartedRunAsync();
        await _fixture.RecordOutput.ExecuteAsync(run.Id, new RecordOutputRequest(_fixture.PostId(run, "Montagem"), 50, null), CancellationToken.None);

        var list = await _client.GetAsync("/api/v1/oee/runs?status=running");
        using var listBody = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        var item = listBody.RootElement.GetProperty("runs")[0];

        var detail = await _client.GetAsync($"/api/v1/oee/runs/{run.Id}");
        using var detailBody = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Equal("Linha 1", item.GetProperty("lineName").GetString());
        Assert.Equal(4, item.GetProperty("measuredPosts").GetInt32());
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        Assert.Equal(4, detailBody.RootElement.GetProperty("posts").GetArrayLength());
        Assert.Equal(50, detailBody.RootElement.GetProperty("posts")[0].GetProperty("produced").GetDouble());
        Assert.Equal("Padrão", detailBody.RootElement.GetProperty("formulaSetName").GetString());
        Assert.True(detailBody.RootElement.GetProperty("posts")[0].GetProperty("pillars").GetProperty("availability").GetDouble() > 99);
    }

    [Fact]
    public async Task Runs_WithAnInvertedPeriod_Is400()
    {
        var response = await _client.GetAsync("/api/v1/oee/runs?from=2026-03-10&to=2026-03-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RunDetail_Unknown_Is404()
    {
        var response = await _client.GetAsync("/api/v1/oee/runs/4040");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
