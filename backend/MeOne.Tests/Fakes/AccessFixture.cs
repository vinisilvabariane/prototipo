using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Fakes;

public sealed class AccessFixture
{
    public const string Password = "strong-password";

    public AccessFixture()
    {
        Departments = new FakeDepartmentRepository(Users);
        Roles = new FakeRoleRepository(Users);
        Scopes = new FakeScopeRepository(Users);

        Ti = Department.Create("TI");
        Departments.Insert(Ti);

        foreach (var definition in RoleCatalog.All)
            Roles.Insert(Role.Create(definition.Name, definition.Description, definition.Permissions));

        Plant = AddScope(ScopeType.Plant, "PLT-1", null);
        AreaA = AddScope(ScopeType.Area, "AREA-A", Plant);
        AreaB = AddScope(ScopeType.Area, "AREA-B", Plant);
        LineA1 = AddScope(ScopeType.Line, "LINE-A1", AreaA);
        StationA1 = AddScope(ScopeType.Workstation, "WS-A1-01", LineA1);
    }

    public FakeUserRepository Users { get; } = new();

    public FakeDepartmentRepository Departments { get; }

    public FakeRoleRepository Roles { get; }

    public FakeScopeRepository Scopes { get; }

    public FakeRefreshTokenRepository Tokens { get; } = new();

    public FakePasswordResetTokenRepository ResetTokens { get; } = new();

    public FakeAccessProfileCache AccessProfiles { get; } = new();

    public FakePasswordHasher Hasher { get; } = new();

    public FakeRefreshTokenGenerator RefreshTokens { get; } = new();

    public FakeAccessTokenIssuer AccessTokens { get; } = new();

    public SessionPolicy Policy { get; } = SessionPolicy.Default;

    public Department Ti { get; }

    public Scope Plant { get; }

    public Scope AreaA { get; }

    public Scope AreaB { get; }

    public Scope LineA1 { get; }

    public Scope StationA1 { get; }

    public Role MasterRole => RoleNamed(AccessRoles.Master);

    public Role AccessManagerRole => RoleNamed("gestor-acessos");

    public Role SupervisorRole => RoleNamed("supervisor-producao");

    public Role OperatorRole => RoleNamed("operador");

    public Role RoleNamed(string name) => Roles.Stored.Single(role => role.Name == name);

    public User AddUser(
        uint registration,
        string? email = "john.doe@grupomultilaser.com.br",
        string name = "John Doe Smith",
        params Role[] roles)
    {
        var user = User.Create(registration, name, email, Hasher.Hash(Password), Ti, modifiedBy: 1);
        Users.Insert(user);

        foreach (var role in roles)
            user.GrantRole(role, null);

        return user;
    }

    public User AddAdministrator(uint registration = 9001) =>
        AddUser(registration, $"admin{registration}@grupomultilaser.com.br", "Admin User Account", MasterRole);

    public RefreshToken AddRefreshToken(User user, string raw, DateTime? expiresAtUtc = null)
    {
        var token = RefreshToken.Issue(
            user.Id,
            RefreshTokens.Hash(raw),
            expiresAtUtc ?? DateTime.UtcNow.AddHours(1));

        Tokens.Insert(token);
        return token;
    }

    private Scope AddScope(ScopeType type, string code, Scope? parent)
    {
        var scope = Scope.Create(type, code, code, parent?.Id);
        Scopes.Insert(scope);
        return scope;
    }
}
