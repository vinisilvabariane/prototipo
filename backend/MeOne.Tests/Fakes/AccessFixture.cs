using MeOne.Domain.Entities;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Fakes;

public sealed class AccessFixture
{
    public const string Password = "strong-password";

    public AccessFixture()
    {
        Departments = new FakeDepartmentRepository(Users);
        Roles = new FakeRoleRepository(Users);

        Ti = Department.Create("TI");
        Departments.Insert(Ti);

        MasterRole = Role.Create(AccessRoles.Master, "Administration");
        OperatorRole = Role.Create("operator", "Line operation");
        Roles.Insert(MasterRole, OperatorRole);
    }

    public FakeUserRepository Users { get; } = new();

    public FakeDepartmentRepository Departments { get; }

    public FakeRoleRepository Roles { get; }

    public FakeRefreshTokenRepository Tokens { get; } = new();

    public FakePasswordHasher Hasher { get; } = new();

    public FakeRefreshTokenGenerator RefreshTokens { get; } = new();

    public FakeAccessTokenIssuer AccessTokens { get; } = new();

    public SessionPolicy Policy { get; } = SessionPolicy.Default;

    public Department Ti { get; }

    public Role MasterRole { get; }

    public Role OperatorRole { get; }

    public User AddUser(
        uint registration,
        string? email = "john.doe@grupomultilaser.com.br",
        string name = "John Doe Smith",
        params Role[] roles)
    {
        var user = User.Create(registration, name, email, Hasher.Hash(Password), Ti, modifiedBy: 1);
        Users.Insert(user);

        foreach (var role in roles)
            user.GrantRole(role);

        return user;
    }

    public RefreshToken AddRefreshToken(User user, string raw, DateTime? expiresAtUtc = null)
    {
        var token = RefreshToken.Issue(
            user.Id,
            RefreshTokens.Hash(raw),
            expiresAtUtc ?? DateTime.UtcNow.AddHours(1));

        Tokens.Insert(token);
        return token;
    }
}
