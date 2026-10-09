using IrisWeb.Domain.Entities;
using IrisWeb.Infrastructure.Data.Context;
using IrisWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Tests.Infrastructure;

// Sem banco: o modelo e o SQL saem do provider sem conectar. As tabelas são as do Access Control,
// criadas à mão — nome de coluna errado só apareceria em runtime.
public sealed class AccessMappingTests
{
    private static IrisWebDbContext Context() =>
        new(new DbContextOptionsBuilder<IrisWebDbContext>()
            .UseMySql("Server=localhost;Database=iris;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    private static Dictionary<string, string?> Columns<T>(IrisWebDbContext context, out string? table)
    {
        var entity = context.Model.FindEntityType(typeof(T))!;
        table = entity.GetTableName();
        return entity.GetProperties().ToDictionary(p => p.Name, p => (string?)p.GetColumnName());
    }

    [Fact]
    public void Users_UsaAsColunasDoAccessControl()
    {
        using var context = Context();
        var columns = Columns<User>(context, out var table);

        Assert.Equal("users", table);
        Assert.Equal("registration", columns[nameof(User.Registration)]);
        Assert.Equal("password", columns[nameof(User.PasswordHash)]);
        Assert.Equal("fk_department", columns[nameof(User.DepartmentId)]);
        Assert.Equal("modified_at", columns[nameof(User.ModifiedAt)]);
        Assert.Equal("modified_by", columns[nameof(User.ModifiedBy)]);
        Assert.Equal("active", columns[nameof(User.Active)]);
        Assert.DoesNotContain(nameof(User.RoleNames), columns.Keys);
    }

    [Fact]
    public void Tabelas_DeLigacao_ETokens_UsamOsNomesDoAccessControl()
    {
        using var context = Context();

        var userRoles = Columns<UserRole>(context, out var userRolesTable);
        Assert.Equal("user_roles", userRolesTable);
        Assert.Equal("fk_user", userRoles[nameof(UserRole.UserId)]);
        Assert.Equal("fk_role", userRoles[nameof(UserRole.RoleId)]);

        var userApps = Columns<UserApplication>(context, out var userAppsTable);
        Assert.Equal("user_applications", userAppsTable);
        Assert.Equal("fk_application", userApps[nameof(UserApplication.ApplicationId)]);

        Columns<ClientApplication>(context, out var appsTable);
        Assert.Equal("applications", appsTable);

        Columns<Department>(context, out var departmentsTable);
        Assert.Equal("departments", departmentsTable);

        Columns<Role>(context, out var rolesTable);
        Assert.Equal("roles", rolesTable);

        var tokens = Columns<RefreshToken>(context, out var tokensTable);
        Assert.Equal("refresh_tokens", tokensTable);
        Assert.Equal("token", tokens[nameof(RefreshToken.TokenHash)]);
        Assert.Equal("expires", tokens[nameof(RefreshToken.ExpiresAt)]);
        Assert.Equal("revoked", tokens[nameof(RefreshToken.Revoked)]);
        Assert.Equal("fk_user", tokens[nameof(RefreshToken.UserId)]);
    }

    [Fact]
    public void ConsultaDeUsuario_SempreFiltraDesativados()
    {
        using var context = Context();

        var sql = context.Set<User>().Where(user => user.Registration == 1).ToQueryString();

        Assert.Contains("`active`", sql);
    }

    [Fact]
    public void Busca_TraduzNomeEmailEMatriculaParaSql()
    {
        using var context = Context();

        var sql = UserRepository.ApplySearch(context.Set<User>(), "123").ToQueryString();

        Assert.Contains("`name`", sql);
        Assert.Contains("`email`", sql);
        Assert.Contains("`registration`", sql);
        Assert.Contains("LIKE", sql);
    }

    [Fact]
    public void CarregamentoDeAcesso_TraduzRolesEAplicacoes()
    {
        using var context = Context();

        var sql = context.Set<User>()
            .Include(user => user.Department)
            .Include(user => user.Roles).ThenInclude(link => link.Role)
            .Include(user => user.Applications).ThenInclude(link => link.Application)
            .Where(user => user.Id == 1)
            .ToQueryString();

        Assert.Contains("`user_roles`", sql);
        Assert.Contains("`roles`", sql);
        Assert.Contains("`user_applications`", sql);
        Assert.Contains("`applications`", sql);
        Assert.Contains("`departments`", sql);
    }
}
