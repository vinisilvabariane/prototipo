using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using MeOne.API.Controllers;
using MeOne.API.Errors;
using MeOne.API.Validation;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.WorkInstructions;
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

public sealed class WorkInstructionEndpointTests : IAsyncLifetime
{
    private readonly WorkInstructionFixture _fixture = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services
            .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddApplicationPart(typeof(WorkInstructionsController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(ErrorResponse.From(
                FluentValidationFilter.Title,
                "Corpo da requisição inválido ou mal formatado.")));

        builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        builder.Services.AddApplicationUseCases();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<IWorkInstructionRepository>(_fixture.Instructions);
        builder.Services.AddSingleton<IUserRepository>(_fixture.Access.Users);
        builder.Services.AddSingleton<IWorkInstructionFileStorage>(_fixture.Storage);
        builder.Services.AddScoped<ICurrentActor>(_ => _fixture.AuthorActor);

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

    private static MultipartFormDataContent Form(
        string code = "IT-20261015-AAAAA",
        string title = "Soldar a carcaça",
        (string Text, ulong? Id, bool RemoveImage, bool Image)[]? steps = null,
        bool pdf = true,
        string? notes = null)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(code), "code" },
            { new StringContent(title), "title" },
            { new StringContent("DOC-77"), "documentNumber" },
            { new StringContent("Fone X"), "product" },
            { new StringContent("Elisa"), "preparedBy" },
            { new StringContent("Marcos"), "approvedBy" },
            { new StringContent("Luvas e óculos"), "requiredPpe" },
            { new StringContent("Temperatura de 350 °C"), "processSpecifications" },
        };

        if (notes is not null)
            form.Add(new StringContent(notes), "notes");

        var list = steps ?? [];

        for (var index = 0; index < list.Length; index++)
        {
            form.Add(new StringContent(list[index].Text), $"steps[{index}].text");

            if (list[index].Id is { } id)
                form.Add(new StringContent(id.ToString()), $"steps[{index}].id");

            if (list[index].RemoveImage)
                form.Add(new StringContent("true"), $"steps[{index}].removeImage");

            if (list[index].Image)
            {
                var image = new ByteArrayContent(WorkInstructionFixture.PngBytes);
                image.Headers.ContentType = new("image/png");
                form.Add(image, $"stepImages[{index}]", "step.png");
            }
        }

        if (pdf)
        {
            var file = new ByteArrayContent(WorkInstructionFixture.PdfBytes);
            file.Headers.ContentType = new("application/pdf");
            form.Add(file, "pdf", "it.pdf");
        }

        return form;
    }

    [Fact]
    public async Task Create_BindsTheFormTheStepsTheImagesAndThePdf()
    {
        var response = await _client.PostAsync(
            "/api/v1/work-instructions",
            Form(steps: [("Posicionar", null, false, true), ("Soldar", null, false, false)], notes: "Cuidado"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("IT-20261015-AAAAA", body.RootElement.GetProperty("code").GetString());
        Assert.Equal(2, body.RootElement.GetProperty("stepCount").GetInt32());

        var stored = Assert.Single(_fixture.Instructions.Stored);
        Assert.Equal("Cuidado", stored.Notes);
        Assert.True(stored.Steps[0].HasImage);
        Assert.False(stored.Steps[1].HasImage);
    }

    [Fact]
    public async Task Create_WithoutSteps_IsAccepted()
    {
        var response = await _client.PostAsync("/api/v1/work-instructions", Form());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Empty(_fixture.Instructions.Stored.Single().Steps);
    }

    [Fact]
    public async Task Create_WithoutTheRequiredFields_IsRejectedWithTheEnvelope()
    {
        var response = await _client.PostAsync("/api/v1/work-instructions", Form(title: ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("details", out _));
    }

    [Fact]
    public async Task Create_WithoutThePdf_Is400()
    {
        var response = await _client.PostAsync("/api/v1/work-instructions", Form(pdf: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("PDF", body.RootElement.GetProperty("details").GetString());
    }

    [Fact]
    public async Task GetById_ReturnsTheDetailWithTheSteps()
    {
        var instruction = await _fixture.AddAsync(steps: 2);

        var response = await _client.GetAsync($"/api/v1/work-instructions/{instruction.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Elisa Engenheira", body.RootElement.GetProperty("createdBy").GetString());
        Assert.Equal(2, body.RootElement.GetProperty("steps").GetArrayLength());
    }

    [Fact]
    public async Task GetById_Unknown_Is404()
    {
        var response = await _client.GetAsync("/api/v1/work-instructions/4040");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_BindsPagingAndSearch()
    {
        await _fixture.AddAsync("IT-ALFA", "Soldar a carcaça");
        await _fixture.AddAsync("IT-BETA", "Embalar o produto");

        var response = await _client.GetAsync("/api/v1/work-instructions?page=0&pageSize=5&search=embalar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task NextCode_ReturnsAFreshCode()
    {
        var response = await _client.GetAsync("/api/v1/work-instructions/next-code");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.StartsWith("IT-", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Pdf_IsServedInlineForTheBrowserViewer()
    {
        var instruction = await _fixture.AddAsync("IT-9");

        var response = await _client.GetAsync($"/api/v1/work-instructions/{instruction.Id}/pdf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(WorkInstructionFixture.PdfBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task StepImage_IsServed()
    {
        var instruction = await _fixture.AddAsync(steps: 1, withImages: true);

        var response = await _client.GetAsync($"/api/v1/work-instructions/{instruction.Id}/steps/{instruction.Steps[0].Id}/image");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Update_AppliesTheFormToAnExistingInstruction()
    {
        var instruction = await _fixture.AddAsync(steps: 2);
        var first = instruction.Steps[0];

        var response = await _client.PutAsync(
            $"/api/v1/work-instructions/{instruction.Id}",
            Form(title: "Título novo", steps: [("Passo 1 editado", first.Id, false, true), ("Passo extra", null, false, false)], pdf: false));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Título novo", instruction.Title);
        Assert.Equal(["Passo 1 editado", "Passo extra"], instruction.Steps.Select(step => step.Text));
        Assert.True(instruction.Steps[0].HasImage);
    }

    [Fact]
    public async Task Delete_RemovesIt()
    {
        var instruction = await _fixture.AddAsync();

        var response = await _client.DeleteAsync($"/api/v1/work-instructions/{instruction.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(_fixture.Instructions.Stored);
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
