using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class RoleRepository(MeOneDbContext context)
    : Repository<Role, ulong>(context), IRoleRepository
{
    public override void Update(Role entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<Role?> GetWithPermissionsAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include(role => role.Permissions)
            .FirstOrDefaultAsync(role => role.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, ulong? exceptRoleId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();

        return DbSet.AnyAsync(
            role => role.Name == trimmed && (exceptRoleId == null || role.Id != exceptRoleId),
            cancellationToken);
    }

    public Task<bool> InUseAsync(ulong roleId, CancellationToken cancellationToken) =>
        Context.Set<UserRole>().AnyAsync(link => link.RoleId == roleId, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken) => DbSet.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> ListPageAsync(int skip, int take, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Include(role => role.Permissions)
            .OrderBy(role => role.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Role>> ListOrderedAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Include(role => role.Permissions)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);
}
