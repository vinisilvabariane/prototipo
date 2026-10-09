using System.Text;
using System.Threading.RateLimiting;
using System.Text.Json;
using System.Text.Json.Serialization;
using IrisWeb.API.Auth;
using IrisWeb.API.Errors;
using IrisWeb.API.Health;
using FluentValidation;
using FluentValidation.AspNetCore;
using IrisWeb.Application.Validators.Stations;
using IrisWeb.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Auth;

const string CorsPolicy = "IrisClients";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationUseCases();
builder.Services.AddInfrastructureRepositories();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<ClosePresenceRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage);
        return new BadRequestObjectResult(ErrorResponse.From("Falha de validação", errors));
    };
});

builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

    if (builder.Environment.IsDevelopment() || origins.Contains("*"))
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
    else
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.AddOutputCache(options =>
    options.AddPolicy(HealthEndpoints.CachePolicy, policy => policy.Expire(TimeSpan.FromSeconds(5))));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentActor, HttpContextCurrentActor>();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Seção Jwt ausente no appsettings.");

if (string.IsNullOrWhiteSpace(jwt.Key))
    throw new InvalidOperationException("Jwt:Key nao configurado — defina via user-secrets ou Jwt__Key.");

if (Encoding.UTF8.GetByteCount(jwt.Key) < JwtOptions.MinKeyBytes)
    throw new InvalidOperationException($"Jwt:Key precisa de pelo menos {JwtOptions.MinKeyBytes} bytes para HS256.");

if (jwt.Audiences.Length == 0)
    throw new InvalidOperationException("Jwt:Audiences vazio — nenhum token seria aceito.");

if (jwt.ExpirationInMinutes <= 0)
    throw new InvalidOperationException("Jwt:ExpirationInMinutes tem que ser > 0.");

// O IrisWeb emite os próprios tokens (login local); a validação abaixo aceita só o que ele emite
// com a mesma chave/issuer/audiences — ou o que outro emissor com a mesma configuração emitir.
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudiences = jwt.Audiences,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                if (context.Response.HasStarted) return;

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(ErrorResponse.From(
                    "Nao autenticado",
                    "Sessao ausente ou expirada. Faca login novamente."));
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted) return;

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(ErrorResponse.From(
                    "Acesso negado",
                    "Sua matricula nao tem permissao para esta operacao."));
            },
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(AuthPolicies.MasterOnly, policy => policy.RequireRole(AuthPolicies.MasterRole)));
builder.Services.AddSingleton<IClaimsTransformation, RolesClaimsTransformation>();

var anonymousPermitPerMinute = builder.Configuration.GetValue("RateLimiting:AnonymousCredentialsPerMinute", 30);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            ErrorResponse.From("Muitas tentativas", "Aguarde um minuto e tente de novo."),
            cancellationToken);
    };

    options.AddPolicy(AuthPolicies.AnonymousRateLimit, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = anonymousPermitPerMinute,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        app.Logger.LogError(exception, "Excecao nao tratada em {Path}.", context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var details = app.Environment.IsDevelopment()
            ? BuildDeveloperDetail(exception)
            : "Tente novamente. Se persistir, acione o time de automação.";

        await context.Response.WriteAsJsonAsync(
            ErrorResponse.From("Ocorreu um erro interno no servidor.", details));
    });
});

// Fora do Development, Swagger só liga com "Swagger:Enabled" explícito (env var
// Swagger__Enabled=true) — nunca por padrão. A API expõe rota autenticada de verdade
// (enroll, estações, governança de modelo), e o schema dessas rotas não deve ficar público
// só porque alguém bateu na URL.
var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IrisWeb API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "IrisWeb API";
    });
}

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

var staticContentTypes = new FileExtensionContentTypeProvider();
staticContentTypes.Mappings[".onnx"] = "application/octet-stream";
staticContentTypes.Mappings[".tflite"] = "application/octet-stream";

app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = staticContentTypes });

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();

app.MapControllers();
app.MapIrisHealthEndpoints();

app.MapFallback(async context =>
{
    var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (!File.Exists(indexPath))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

static string BuildDeveloperDetail(Exception? exception)
{
    if (exception is null)
        return "Erro desconhecido";

    return exception.InnerException is null
        ? exception.Message
        : $"{exception.Message}{Environment.NewLine}{exception.InnerException.Message}";
}
