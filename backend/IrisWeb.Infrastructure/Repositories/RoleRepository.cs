using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class RoleRepository(IrisWebDbContext context)
    : Repository<Role, ulong>(context), IRoleRepository
{
    public override void Update(Role entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> NameExistsAsync(string name, ulong? exceptRoleId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();

        return DbSet.AnyAsync(
            role => role.Name == trimmed && (exceptRoleId == null || role.Id != exceptRoleId),
            cancellationToken);
    }

    // Conta atribuição de usuário desativado também: a linha em user_roles continua lá.
    public Task<bool> InUseAsync(ulong roleId, CancellationToken cancellationToken) =>
        Context.Set<UserRole>().AnyAsync(link => link.RoleId == roleId, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken) => DbSet.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> ListPageAsync(int skip, int take, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .OrderBy(role => role.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> ListOrderedAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);
}
