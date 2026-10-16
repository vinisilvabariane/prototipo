using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class ProductionShiftRepository(MeOneDbContext context)
    : Repository<ProductionShift, ulong>(context), IProductionShiftRepository
{
    public override void Update(ProductionShift entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(item => item.Name == name && (exceptId == null || item.Id != exceptId), cancellationToken);

    public async Task<IReadOnlyList<ProductionShift>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Where(item => includeInactive || item.Active)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductionShift>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking().Where(item => ids.Contains(item.Id)).ToListAsync(cancellationToken);
}
