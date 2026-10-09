using System.Reflection;
using IrisWeb.API.Auth;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Application.Interfaces.Collaborators;
using IrisWeb.Application.Interfaces.Departments;
using IrisWeb.Application.Interfaces.Roles;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Application.Interfaces.FaceRecognition;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Application.Interfaces.Stations;

namespace IrisWeb.Tests.Infrastructure;

public sealed class DependencyInjectionConventionTests
{
    private static readonly Assembly ApplicationAssembly = typeof(IListStationTimelineUseCase).Assembly;

    private static readonly Assembly DomainAssembly = typeof(IStationPresenceRepository).Assembly;

    private static string PastaTemporariaDeArtefatos() =>
        Path.Combine(Path.GetTempPath(), "iris-tests", "di", Guid.NewGuid().ToString("N"));

    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Iris"] = "Server=localhost;Port=3306;Database=iris;User=iris;Password=iris;",
                ["ModelStorage:Path"] = PastaTemporariaDeArtefatos(),
                ["Sync:BaseUrl"] = "http://localhost:5000",
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, HttpContextCurrentActor>();

        // Registrado no Program.cs (o emissor mora na API, ao lado da validação do JWT).
        services.AddSingleton(new JwtOptions { Key = new string('k', JwtOptions.MinKeyBytes), Audiences = ["iris-web"] });
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        services.AddApplicationUseCases();
        services.AddInfrastructureRepositories();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    private static Type[] ApplicationUseCaseContracts() => [.. ApplicationAssembly
        .GetTypes()
        .Where(type => type is { IsInterface: true, IsGenericTypeDefinition: false })
        .Where(type => type.Name.StartsWith('I') && type.Name.EndsWith("UseCase", StringComparison.Ordinal))];

    private static Type[] DomainRepositoryContracts() => [.. DomainAssembly
        .GetTypes()
        .Where(type => type is { IsInterface: true, IsGenericTypeDefinition: false })
        .Where(type => type.Name.StartsWith('I') && type.Name.EndsWith("Repository", StringComparison.Ordinal))];

    [Fact]
    public void A_varredura_enxerga_os_contratos_das_duas_camadas()
    {
        Assert.NotEmpty(ApplicationUseCaseContracts());
        Assert.NotEmpty(DomainRepositoryContracts());
    }

    [Fact]
    public void Os_contratos_em_uso_resolvem_dentro_de_um_escopo()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ILoginUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IRefreshSessionUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<ILogoutUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IValidateCredentialsUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICreateUserUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IResetPasswordUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IAssignRoleUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IDeleteDepartmentUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IGetCollaboratorUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUserRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IRefreshTokenRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IClosePresenceUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMatchFaceUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IEnrollFaceUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IListModelsUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICreateDownloadUrlUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICompleteUploadUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IApproveModelVersionUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetService<IStationPresenceRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IStationVacancyRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IFaceEmbeddingRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IModelCatalogRepository>());
    }

    [Fact]
    public void Nenhum_contrato_e_registrado_duas_vezes()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Iris"] = "Server=localhost;Port=3306;Database=iris;User=iris;Password=iris;",
                ["ModelStorage:Path"] = PastaTemporariaDeArtefatos(),
                ["Sync:BaseUrl"] = "http://localhost:5000",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddApplicationUseCases();
        services.AddInfrastructureRepositories();
        services.AddInfrastructure(configuration);

        var contratos = ApplicationUseCaseContracts()
            .Concat(DomainRepositoryContracts())
            .ToHashSet();

        var duplicados = services
            .Where(descriptor => contratos.Contains(descriptor.ServiceType))
            .GroupBy(descriptor => descriptor.ServiceType)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key.Name)
            .ToArray();

        Assert.True(
            duplicados.Length == 0,
            $"Registro duplicado — a convencao rodou duas vezes sobre: {string.Join(", ", duplicados)}");
    }

    [Fact]
    public void Todo_contrato_de_use_case_e_de_repository_tem_implementacao_registrada()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var orfaos = ApplicationUseCaseContracts()
            .Concat(DomainRepositoryContracts())
            .Where(contrato => scope.ServiceProvider.GetService(contrato) is null)
            .Select(contrato => contrato.Name)
            .ToArray();

        Assert.True(
            orfaos.Length == 0,
            "Sem par nome/interface a classe nao entra no container e o erro so aparece na " +
            $"primeira requisicao. Sem implementacao registrada: {string.Join(", ", orfaos)}");
    }
}
