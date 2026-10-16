using System.Text;
using System.Threading.RateLimiting;
using System.Text.Json;
using System.Text.Json.Serialization;
using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.API.Health;
using FluentValidation;
using MeOne.API.Validation;
using MeOne.Application.Validators.Auth;
using MeOne.Infrastructure;
using MeOne.API.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;

const string CorsPolicy = "MeOneClients";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddApplicationUseCases();
builder.Services.AddInfrastructureRepositories();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers(options => options.Filters.Add<FluentValidationFilter>())
    .AddJsonOptions(options =>

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddOpenApi();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(ErrorResponse.From(
            FluentValidationFilter.Title,
            "Corpo da requisição inválido ou mal formatado."));
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
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<AccessProfileCache>();
builder.Services.AddSingleton<IAccessProfileCache>(provider => provider.GetRequiredService<AccessProfileCache>());
builder.Services.AddScoped<ICurrentActor, HttpContextCurrentActor>();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt section is missing from configuration.");

if (string.IsNullOrWhiteSpace(jwt.Key))
    throw new InvalidOperationException("Jwt:Key is not configured — set it via user-secrets or Jwt__Key.");

if (Encoding.UTF8.GetByteCount(jwt.Key) < JwtOptions.MinKeyBytes)
    throw new InvalidOperationException($"Jwt:Key must be at least {JwtOptions.MinKeyBytes} bytes long for HS256.");

if (jwt.Audiences.Length == 0)
    throw new InvalidOperationException("Jwt:Audiences is empty — no token would be accepted.");

if (jwt.ExpirationInMinutes <= 0)
    throw new InvalidOperationException("Jwt:ExpirationInMinutes must be greater than 0.");

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = TokenClaims.Name,
            RoleClaimType = TokenClaims.Roles,
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
            OnTokenValidated = async context =>
            {
                var stamp = context.Principal?.FindFirst(TokenClaims.SecurityStamp)?.Value;

                if (TokenClaims.UserId(context.Principal) is not { } userId || stamp is null)
                {
                    context.Fail("Token sem identificação de sessão.");
                    return;
                }

                var profile = await context.HttpContext.RequestServices
                    .GetRequiredService<AccessProfileCache>()
                    .GetAsync(userId, context.HttpContext.RequestAborted);

                if (profile is not { Active: true } || profile.SecurityStamp != stamp)
                    context.Fail("Sessão revogada.");
            },
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

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

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

        app.Logger.LogError(exception, "Unhandled exception at {Path}.", context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var details = app.Environment.IsDevelopment()
            ? BuildDeveloperDetail(exception)
            : "Tente novamente. Se persistir, acione o time de automação.";

        await context.Response.WriteAsJsonAsync(
            ErrorResponse.From("Ocorreu um erro interno no servidor.", details));
    });
});

var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MeOne API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "MeOne API";
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

app.UseSecurityHeaders();

app.UseStaticFiles();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseOutputCache();

app.MapControllers();
app.MapMeOneHealthEndpoints();

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
        return "Unknown error";

    return exception.InnerException is null
        ? exception.Message
        : $"{exception.Message}{Environment.NewLine}{exception.InnerException.Message}";
}
