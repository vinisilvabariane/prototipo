using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using MeOne.API.Controllers;
using MeOne.API.Errors;
using MeOne.API.Validation;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Application.Validators.Auth;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;
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

public sealed class WorkflowEndpointTests : IAsyncLifetime
{
    private readonly WorkflowFixture _fixture = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services
            .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddApplicationPart(typeof(WorkflowsController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(ErrorResponse.From(
                FluentValidationFilter.Title,
                "Corpo da requisição inválido ou mal formatado.")));

        builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        builder.Services.AddApplicationUseCases();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<IWorkflowRepository>(_fixture.Workflows);
        builder.Services.AddSingleton<IUserRepository>(_fixture.Users);
        builder.Services.AddSingleton<IWorkflowFileStorage>(_fixture.Storage);
        builder.Services.AddScoped<ICurrentActor>(provider =>
        {
            var header = provider.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request.Headers["X-Test-User"];
            var userId = header?.ToString();
            var user = _fixture.Users.Stored.Single(candidate => candidate.Id.ToString() == userId);

            return WorkflowFixture.Actor(user);
        });

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

    private HttpRequestMessage Request(HttpMethod method, string url, ulong userId, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Add("X-Test-User", userId.ToString());
        return request;
    }

    private ulong Creator => _fixture.Creator.Id;

    private ulong Alice => _fixture.Alice.Id;

    private ulong Carol => _fixture.Carol.Id;

    private MultipartFormDataContent Form(
        string title = "Review the plan",
        string? description = "Details",
        IEnumerable<ulong>? assignees = null,
        int files = 0)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(title), "title" },
            { new StringContent(description ?? string.Empty), "description" },
            { new StringContent("high"), "priority" },
            { new StringContent("standards"), "category" },
            { new StringContent(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3).ToString("yyyy-MM-dd")), "dueDate" },
        };

        foreach (var id in assignees ?? [])
            form.Add(new StringContent(id.ToString()), "assigneeIds");

        for (var index = 0; index < files; index++)
        {
            var file = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            file.Headers.ContentType = new("application/pdf");
            form.Add(file, "attachments", $"plan{index}.pdf");
        }

        return form;
    }

    [Fact]
    public async Task Create_BindsTheMultipartFormFieldsAssigneesAndFiles()
    {
        var response = await _client.SendAsync(Request(
            HttpMethod.Post,
            "/api/v1/workflows",
            Creator,
            Form(assignees: [_fixture.Alice.Id, _fixture.Bob.Id], files: 2)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("high", body.RootElement.GetProperty("priority").GetString());
        Assert.Equal("standards", body.RootElement.GetProperty("category").GetString());
        Assert.Equal("inProgress", body.RootElement.GetProperty("status").GetString());
        Assert.EndsWith("Z", body.RootElement.GetProperty("createdAt").GetString());

        var stored = Assert.Single(_fixture.Workflows.Stored);
        Assert.Equal(3, stored.Assignees.Count);
        Assert.Equal(2, stored.Attachments.Count);
        Assert.Equal(["plan0.pdf", "plan1.pdf"], stored.Attachments.Select(attachment => attachment.OriginalName).Order());
    }

    [Fact]
    public async Task Create_AcceptsAnEmptyDescription()
    {
        var response = await _client.SendAsync(Request(HttpMethod.Post, "/api/v1/workflows", Creator, Form(description: "")));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(string.Empty, _fixture.Workflows.Stored.Single().Description);
    }

    [Fact]
    public async Task Create_WithoutATitle_IsRejectedWithTheStandardEnvelope()
    {
        var response = await _client.SendAsync(Request(HttpMethod.Post, "/api/v1/workflows", Creator, Form(title: "")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("error", out _));
        Assert.True(body.RootElement.TryGetProperty("details", out _));
        Assert.Empty(_fixture.Workflows.Stored);
    }

    [Fact]
    public async Task GetById_ReturnsCamelCaseEnumsAndWhatTheActorCanDo()
    {
        var workflow = _fixture.AddWorkflow();

        var response = await _client.SendAsync(Request(HttpMethod.Get, $"/api/v1/workflows/{workflow.Id}", Alice));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("inProgress", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("procedures", body.RootElement.GetProperty("category").GetString());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", body.RootElement.GetProperty("dueDate").GetString());
        Assert.True(body.RootElement.GetProperty("permissions").GetProperty("canApprove").GetBoolean());
        Assert.Equal("approval", body.RootElement.GetProperty("steps")[0].GetProperty("action").GetString());
    }

    [Fact]
    public async Task GetById_Unknown_Is404WithTheEnvelope()
    {
        var response = await _client.SendAsync(Request(HttpMethod.Get, "/api/v1/workflows/4040", Alice));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Workflow não encontrado.", body.RootElement.GetProperty("details").GetString());
    }

    [Fact]
    public async Task List_BindsTheFilterByName()
    {
        _fixture.AddWorkflow("Mine");
        _fixture.AddWorkflow("Alice's", [_fixture.Creator], _fixture.Alice);

        var response = await _client.SendAsync(Request(
            HttpMethod.Get,
            "/api/v1/workflows?filter=mine&page=0&pageSize=5&search=",
            Alice));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal("Alice's", body.RootElement.GetProperty("items")[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task List_WithAnInvalidPageSize_Is400()
    {
        var response = await _client.SendAsync(Request(HttpMethod.Get, "/api/v1/workflows?pageSize=0", Alice));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ByAnAssignee_Is200_AndByAnOutsider_Is403()
    {
        var workflow = _fixture.AddWorkflow();
        var json = JsonContent.Create(new { justification = "Looks good" });

        var forbidden = await _client.SendAsync(Request(HttpMethod.Post, $"/api/v1/workflows/{workflow.Id}/approve", Carol, json));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var ok = await _client.SendAsync(Request(
            HttpMethod.Post,
            $"/api/v1/workflows/{workflow.Id}/approve",
            Alice,
            JsonContent.Create(new { justification = "Looks good" })));

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        using var body = JsonDocument.Parse(await ok.Content.ReadAsStringAsync());
        Assert.Equal("inProgress", body.RootElement.GetProperty("status").GetString());
        Assert.Equal(WorkflowOutcomeTexts.ApprovalRecorded, body.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Approve_WithoutJustification_Is400()
    {
        var workflow = _fixture.AddWorkflow();

        var response = await _client.SendAsync(Request(
            HttpMethod.Post,
            $"/api/v1/workflows/{workflow.Id}/approve",
            Alice,
            JsonContent.Create(new { justification = "  " })));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReturnToAssignee_BindsTheStepAndAnswers204()
    {
        var workflow = _fixture.AddWorkflow();

        await _client.SendAsync(Request(
            HttpMethod.Post,
            $"/api/v1/workflows/{workflow.Id}/reject",
            Alice,
            JsonContent.Create(new { justification = "Wrong tolerance" })));

        var step = workflow.Steps.Single(candidate => candidate.Action == MeOne.Domain.Enums.WorkflowAction.Rejection);

        var response = await _client.SendAsync(Request(
            HttpMethod.Post,
            $"/api/v1/workflows/{workflow.Id}/steps/{step.Id}/return-to-assignee",
            Creator,
            JsonContent.Create(new { justification = "It is within spec" })));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var messages = await _client.SendAsync(Request(
            HttpMethod.Get,
            $"/api/v1/workflows/{workflow.Id}/steps/{step.Id}/messages",
            Alice));

        using var body = JsonDocument.Parse(await messages.Content.ReadAsStringAsync());
        Assert.Equal("It is within spec", body.RootElement[0].GetProperty("text").GetString());
        Assert.Equal("Creator One", body.RootElement[0].GetProperty("userName").GetString());
    }

    [Fact]
    public async Task Attachment_IsServedAsADownload()
    {
        var created = await _client.SendAsync(Request(
            HttpMethod.Post,
            "/api/v1/workflows",
            Creator,
            Form(assignees: [_fixture.Alice.Id], files: 1)));

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var workflow = _fixture.Workflows.Stored.Single();
        var attachment = workflow.Attachments.Single();

        var response = await _client.SendAsync(Request(
            HttpMethod.Get,
            $"/api/v1/workflows/{workflow.Id}/attachments/{attachment.Id}",
            Alice));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("plan0.pdf", response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await response.Content.ReadAsByteArrayAsync());
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
