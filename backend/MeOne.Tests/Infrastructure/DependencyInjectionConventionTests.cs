using System.Reflection;
using MeOne.API.Auth;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Departments;
using MeOne.Application.Interfaces.Roles;
using MeOne.Application.Interfaces.Users;

namespace MeOne.Tests.Infrastructure;

public sealed class DependencyInjectionConventionTests
{
    private static readonly Assembly ApplicationAssembly = typeof(ILoginUseCase).Assembly;

    private static readonly Assembly DomainAssembly = typeof(IUserRepository).Assembly;

    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MeOne"] = "Server=localhost;Port=3306;Database=meone;User=meone;Password=meone;",
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, HttpContextCurrentActor>();

        services.AddSingleton(new JwtOptions { Key = new string('k', JwtOptions.MinKeyBytes), Audiences = ["meone"] });
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
    public void Scan_FindsContractsInBothLayers()
    {
        Assert.NotEmpty(ApplicationUseCaseContracts());
        Assert.NotEmpty(DomainRepositoryContracts());
    }

    [Fact]
    public void ContractsInUse_ResolveWithinScope()
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
        Assert.NotNull(scope.ServiceProvider.GetService<IUserRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IRefreshTokenRepository>());
    }

    [Fact]
    public void NoContract_IsRegisteredTwice()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MeOne"] = "Server=localhost;Port=3306;Database=meone;User=meone;Password=meone;",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddApplicationUseCases();
        services.AddInfrastructureRepositories();
        services.AddInfrastructure(configuration);

        var contracts = ApplicationUseCaseContracts()
            .Concat(DomainRepositoryContracts())
            .ToHashSet();

        var duplicates = services
            .Where(descriptor => contracts.Contains(descriptor.ServiceType))
            .GroupBy(descriptor => descriptor.ServiceType)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.Name)
            .ToArray();

        Assert.True(
            duplicates.Length == 0,
            $"Duplicate registration — the convention ran twice over: {string.Join(", ", duplicates)}");
    }

    [Fact]
    public void EveryUseCaseAndRepositoryContract_HasRegisteredImplementation()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var orphans = ApplicationUseCaseContracts()
            .Concat(DomainRepositoryContracts())
            .Where(contract => scope.ServiceProvider.GetService(contract) is null)
            .Select(contract => contract.Name)
            .ToArray();

        Assert.True(
            orphans.Length == 0,
            "Without a name/interface pair the class is not registered and the error only shows up on the " +
            $"first request. No registered implementation: {string.Join(", ", orphans)}");
    }
}
