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

public sealed class ProductionEndpointTests : IAsyncLifetime
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
            .AddApplicationPart(typeof(ProductionRunsController).Assembly)
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

    private static object Post(long id, string name, string kind = "standard", int x = 0, bool critical = false, decimal? target = null) => new
    {
        id,
        name,
        kind,
        x,
        y = 0,
        width = 220,
        height = 140,
        rotation = 0,
        isCritical = critical,
        targetPerHour = target,
        operation = (string?)null,
    };

    private async Task<(ulong LayoutId, JsonElement Layout)> SavedLayoutAsync()
    {
        var created = await _client.PostAsJsonAsync("/api/v1/production/layouts", new { lineId = _fixture.Line.Id, productId = _fixture.Product.Id, shiftId = _fixture.Shift.Id });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = createdBody.RootElement.GetProperty("id").GetUInt64();

        var saved = await _client.PutAsJsonAsync($"/api/v1/production/layouts/{id}", new
        {
            posts = new[]
            {
                Post(-1, "Montagem", critical: true, target: 100),
                Post(-2, "Maquina A", "machine", 300, target: 50),
            },
            decorations = new[]
            {
                new { id = -1L, kind = "belt", x = 0, y = 200, width = 600, height = 40, rotation = 0, label = "Esteira", locked = true },
            },
        });
        Assert.Equal(HttpStatusCode.NoContent, saved.StatusCode);

        var detail = await _client.GetAsync($"/api/v1/production/layouts/{id}");
        using var body = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        return (id, body.RootElement.Clone());
    }

    [Fact]
    public async Task Layout_IsCreatedSavedWithTemporaryIdsAndReadBack()
    {
        var (_, layout) = await SavedLayoutAsync();

        var posts = layout.GetProperty("posts");
        Assert.Equal(2, posts.GetArrayLength());
        Assert.Equal("Montagem", posts[0].GetProperty("name").GetString());
        Assert.True(posts[0].GetProperty("id").GetUInt64() > 0);
        Assert.Equal("machine", posts[1].GetProperty("kind").GetString());
        Assert.Equal(50m, posts[1].GetProperty("targetPerHour").GetDecimal());
        Assert.Equal("belt", layout.GetProperty("decorations")[0].GetProperty("kind").GetString());
        Assert.True(layout.GetProperty("decorations")[0].GetProperty("locked").GetBoolean());
        Assert.Equal("Linha 1", layout.GetProperty("lineName").GetString());
        Assert.True(layout.GetProperty("canManage").GetBoolean());
    }

    [Fact]
    public async Task Layout_WithRepeatedPostNames_IsRejectedWithTheMessage()
    {
        var (id, _) = await SavedLayoutAsync();

        var response = await _client.PutAsJsonAsync($"/api/v1/production/layouts/{id}", new
        {
            posts = new[] { Post(-1, "Solda"), Post(-2, "solda") },
            decorations = Array.Empty<object>(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("repetido", body.RootElement.GetProperty("details").GetString());
    }

    [Fact]
    public async Task Layout_WithAnUnknownPostKind_Is400()
    {
        var (id, _) = await SavedLayoutAsync();

        var response = await _client.PutAsJsonAsync($"/api/v1/production/layouts/{id}", new
        {
            posts = new[] { Post(-1, "Solda", "wizard") },
            decorations = Array.Empty<object>(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Layout_Unknown_Is404()
    {
        var response = await _client.GetAsync("/api/v1/production/layouts/4040");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Setup_ListsAndRejectsDuplicatedNames()
    {
        var duplicate = await _client.PostAsJsonAsync("/api/v1/production/lines", new { name = "linha 1", active = true });
        var created = await _client.PostAsJsonAsync("/api/v1/production/lines", new { name = "Linha 2", active = true });
        var shift = await _client.PostAsJsonAsync("/api/v1/production/shifts", new { name = "Noite", startTime = "22:00:00", endTime = "06:00:00", active = true });
        var list = await _client.GetAsync("/api/v1/production/lines");

        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.Created, shift.StatusCode);
        using var body = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(2, body.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task Run_FollowsThePreparationStartRecordingAndProductivityFlow()
    {
        var (layoutId, layout) = await SavedLayoutAsync();
        var montagem = layout.GetProperty("posts")[0].GetProperty("id").GetUInt64();

        var created = await _client.PostAsJsonAsync("/api/v1/production/runs", new { layoutId });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var runId = createdBody.RootElement.GetProperty("id").GetUInt64();
        Assert.Equal("preparing", createdBody.RootElement.GetProperty("status").GetString());

        var early = await _client.PostAsync($"/api/v1/production/runs/{runId}/start", null);
        Assert.Equal(HttpStatusCode.BadRequest, early.StatusCode);

        var allocate = await _client.PostAsJsonAsync($"/api/v1/production/runs/{runId}/posts/{montagem}/allocation", new { userId = _fixture.Ana.Id });
        Assert.Equal(HttpStatusCode.NoContent, allocate.StatusCode);

        var start = await _client.PostAsync($"/api/v1/production/runs/{runId}/start", null);
        Assert.Equal(HttpStatusCode.NoContent, start.StatusCode);

        var output = await _client.PostAsJsonAsync($"/api/v1/production/runs/{runId}/outputs", new { postId = montagem, quantity = 70 });
        Assert.Equal(HttpStatusCode.NoContent, output.StatusCode);

        var detail = await _client.GetAsync($"/api/v1/production/runs/{runId}");
        using var body = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Equal("running", body.RootElement.GetProperty("status").GetString());
        Assert.Equal(70, body.RootElement.GetProperty("totalProduced").GetInt32());
        var post = body.RootElement.GetProperty("posts").EnumerateArray().Single(item => item.GetProperty("id").GetUInt64() == montagem);
        Assert.Equal("Ana Operadora", post.GetProperty("allocation").GetProperty("userName").GetString());
        Assert.Equal(70, post.GetProperty("produced").GetInt32());
        Assert.Equal("Ana Operadora", body.RootElement.GetProperty("people")[0].GetProperty("userName").GetString());

        var running = await _client.GetAsync("/api/v1/production/runs?status=running");
        using var runningBody = JsonDocument.Parse(await running.Content.ReadAsStringAsync());
        Assert.Equal(1, runningBody.RootElement.GetProperty("totalCount").GetInt32());

        var close = await _client.PostAsync($"/api/v1/production/runs/{runId}/close", null);
        Assert.Equal(HttpStatusCode.NoContent, close.StatusCode);
    }

    [Fact]
    public async Task Run_RejectsAnUnknownStatusFilterAndAnInvalidQuantity()
    {
        var (layoutId, layout) = await SavedLayoutAsync();
        var montagem = layout.GetProperty("posts")[0].GetProperty("id").GetUInt64();
        var created = await _client.PostAsJsonAsync("/api/v1/production/runs", new { layoutId });
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var runId = createdBody.RootElement.GetProperty("id").GetUInt64();

        var badStatus = await _client.GetAsync("/api/v1/production/runs?status=nonsense");
        var badQuantity = await _client.PostAsJsonAsync($"/api/v1/production/runs/{runId}/outputs", new { postId = montagem, quantity = -5 });

        Assert.Equal(HttpStatusCode.BadRequest, badStatus.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badQuantity.StatusCode);
    }

    [Fact]
    public async Task Candidates_ReturnThePeopleWithTheirAllocationState()
    {
        var (layoutId, _) = await SavedLayoutAsync();
        var created = await _client.PostAsJsonAsync("/api/v1/production/runs", new { layoutId });
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var runId = createdBody.RootElement.GetProperty("id").GetUInt64();

        var response = await _client.GetAsync($"/api/v1/production/runs/{runId}/candidates?search=ana");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Ana Operadora", body.RootElement[0].GetProperty("name").GetString());
        Assert.Equal(0, body.RootElement[0].GetProperty("machinePosts").GetInt32());
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
