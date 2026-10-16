using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using MeOne.API.Controllers;
using MeOne.API.Errors;
using MeOne.API.Validation;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
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

public sealed class SupplierEndpointTests : IAsyncLifetime
{
    private readonly SupplierFixture _fixture = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        builder.Services
            .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
            .AddApplicationPart(typeof(SuppliersController).Assembly)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(ErrorResponse.From(
                FluentValidationFilter.Title,
                "Corpo da requisição inválido ou mal formatado.")));

        builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        builder.Services.AddApplicationUseCases();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddSingleton<ISupplierRepository>(_fixture.Suppliers);
        builder.Services.AddSingleton<IUserRepository>(_fixture.Access.Users);
        builder.Services.AddSingleton<ISupplierFileStorage>(_fixture.Storage);
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

    private static object SupplierBody(string sapCode = "10001", string name = "Plásticos Alfa", string? taxId = SupplierFixture.ValidCnpj) => new
    {
        sapCode,
        name,
        branch = "Matriz",
        multiBranch = "blueMg",
        origin = "domestic",
        taxId,
        corporateName = "Alfa Ltda",
        criticality = "high",
        zipCode = "30110-000",
        district = "Centro",
        street = "Rua A",
        number = "100",
        contactName = "Ana",
        phone = "31 99999-0000",
        email = "contato@alfa.com",
    };

    private static MultipartFormDataContent DocumentForm(string kind, byte[]? bytes = null, string? ownerId = null, string? version = null)
    {
        var form = new MultipartFormDataContent { { new StringContent(kind), "kind" } };

        if (ownerId is not null)
            form.Add(new StringContent(ownerId), "ownerId");

        if (version is not null)
            form.Add(new StringContent(version), "version");

        if (bytes is not null)
        {
            var file = new ByteArrayContent(bytes);
            file.Headers.ContentType = new("application/pdf");
            form.Add(file, "file", "certificado.pdf");
        }

        return form;
    }

    [Fact]
    public async Task Create_BindsTheJsonAndReturns201()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/suppliers", SupplierBody());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("10001", body.RootElement.GetProperty("sapCode").GetString());
        Assert.Equal("blueMg", body.RootElement.GetProperty("multiBranch").GetString());

        var stored = Assert.Single(_fixture.Suppliers.Stored);
        Assert.Equal("11222333000181", stored.TaxId);
    }

    [Fact]
    public async Task Create_WithAnInvalidCnpj_IsRejectedWithTheEnvelope()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/suppliers", SupplierBody(taxId: "11.222.333/0001-82"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Contains("CNPJ", body.RootElement.GetProperty("details").GetString());
        Assert.Empty(_fixture.Suppliers.Stored);
    }

    [Fact]
    public async Task Create_WithADuplicatedSapCode_Is400()
    {
        await _fixture.AddAsync("10001");

        var response = await _client.PostAsJsonAsync("/api/v1/suppliers", SupplierBody("10001"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsTheDetailWithTheItems()
    {
        var supplier = await _fixture.AddAsync();
        await _client.PostAsJsonAsync($"/api/v1/suppliers/{supplier.Id}/tools", new { name = "Molde 1", assetTag = "AF-1" });

        var response = await _client.GetAsync($"/api/v1/suppliers/{supplier.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Quirino Qualidade", body.RootElement.GetProperty("createdBy").GetString());
        Assert.Equal("Molde 1", body.RootElement.GetProperty("tools")[0].GetProperty("name").GetString());
        Assert.True(body.RootElement.GetProperty("canEdit").GetBoolean());
    }

    [Fact]
    public async Task GetById_Unknown_Is404()
    {
        var response = await _client.GetAsync("/api/v1/suppliers/4040");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task List_BindsFiltersAndPaging()
    {
        await _fixture.AddAsync("1", "Alfa");
        await _fixture.AddAsync("2", "Beta", MeOne.Domain.Enums.MultiBranch.GigaAm);

        var response = await _client.GetAsync("/api/v1/suppliers?page=0&pageSize=5&multiBranch=gigaAm");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal("Beta", body.RootElement.GetProperty("items")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task SelfAssessment_BindsTheDateAndReturnsTheExpiry()
    {
        var supplier = await _fixture.AddAsync();

        var response = await _client.PostAsJsonAsync($"/api/v1/suppliers/{supplier.Id}/self-assessments", new
        {
            responsibleId = _fixture.Other.Id,
            assessedOn = "2026-10-15",
            score = 88.5,
            status = "approved",
            editableLink = "https://example.com/doc",
            documentCode = "FORM-1",
            revision = 2,
            needsOnSiteAudit = true,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("2027-10-16", body.RootElement.GetProperty("expiresOn").GetString());
        Assert.Equal("Olavo Outro", body.RootElement.GetProperty("responsibleName").GetString());
    }

    [Fact]
    public async Task Audit_WithAnInvalidValidity_IsRejected()
    {
        var supplier = await _fixture.AddAsync();

        var response = await _client.PostAsJsonAsync($"/api/v1/suppliers/{supplier.Id}/audits", new
        {
            responsibleId = _fixture.Other.Id,
            auditedOn = "2026-10-15",
            validityMonths = 5,
            status = "approved",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tool_CanBeUpdatedAndRemoved()
    {
        var supplier = await _fixture.AddAsync();
        var created = await _client.PostAsJsonAsync($"/api/v1/suppliers/{supplier.Id}/tools", new { name = "Molde" });
        using var body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var toolId = body.RootElement.GetProperty("id").GetUInt64();

        var update = await _client.PutAsJsonAsync($"/api/v1/suppliers/{supplier.Id}/tools/{toolId}", new { name = "Molde 2" });
        var delete = await _client.DeleteAsync($"/api/v1/suppliers/{supplier.Id}/tools/{toolId}");

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(supplier.Tools);
    }

    [Fact]
    public async Task Document_UploadsThenDownloadsThroughMultipart()
    {
        var supplier = await _fixture.AddAsync();

        var upload = await _client.PostAsync(
            $"/api/v1/suppliers/{supplier.Id}/documents",
            DocumentForm("certificate", SupplierFixture.PdfBytes, version: "3"));

        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        using var body = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());
        Assert.Equal("certificate", body.RootElement.GetProperty("kind").GetString());
        Assert.Equal("certificado.pdf", body.RootElement.GetProperty("name").GetString());
        var documentId = body.RootElement.GetProperty("id").GetUInt64();

        var download = await _client.GetAsync($"/api/v1/suppliers/{supplier.Id}/documents/{documentId}/file");

        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal("application/pdf", download.Content.Headers.ContentType?.MediaType);
        Assert.Equal(SupplierFixture.PdfBytes, await download.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Document_ForAToolDrawing_BindsTheOwner()
    {
        var supplier = await _fixture.AddAsync();
        var tool = await _client.PostAsJsonAsync($"/api/v1/suppliers/{supplier.Id}/tools", new { name = "Molde" });
        using var toolBody = JsonDocument.Parse(await tool.Content.ReadAsStringAsync());
        var toolId = toolBody.RootElement.GetProperty("id").GetUInt64();

        var upload = await _client.PostAsync(
            $"/api/v1/suppliers/{supplier.Id}/documents",
            DocumentForm("toolDrawing", SupplierFixture.PdfBytes, ownerId: toolId.ToString()));

        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        Assert.Equal(toolId, supplier.Documents.Single().OwnerId);
    }

    [Fact]
    public async Task Document_WithoutAFile_Is400()
    {
        var supplier = await _fixture.AddAsync();

        var response = await _client.PostAsync($"/api/v1/suppliers/{supplier.Id}/documents", DocumentForm("certificate"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Document_WithAnUnknownKind_Is400()
    {
        var supplier = await _fixture.AddAsync();

        var response = await _client.PostAsync(
            $"/api/v1/suppliers/{supplier.Id}/documents",
            DocumentForm("nonsense", SupplierFixture.PdfBytes));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_fixture.Storage.StoredNames);
    }

    [Fact]
    public async Task Document_CanBeRemoved()
    {
        var supplier = await _fixture.AddAsync();
        var upload = await _client.PostAsync($"/api/v1/suppliers/{supplier.Id}/documents", DocumentForm("certificate", SupplierFixture.PdfBytes));
        using var body = JsonDocument.Parse(await upload.Content.ReadAsStringAsync());

        var response = await _client.DeleteAsync($"/api/v1/suppliers/{supplier.Id}/documents/{body.RootElement.GetProperty("id").GetUInt64()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(supplier.Documents);
        Assert.Empty(_fixture.Storage.StoredNames);
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
