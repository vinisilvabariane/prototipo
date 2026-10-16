using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class ProductionProductRepository(MeOneDbContext context)
    : Repository<ProductionProduct, ulong>(context), IProductionProductRepository
{
    public override void Update(ProductionProduct entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(item => item.Code == normalizedCode && (exceptId == null || item.Id != exceptId), cancellationToken);

    public async Task<IReadOnlyList<ProductionProduct>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Where(item => includeInactive || item.Active)
            .OrderBy(item => item.Code)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductionProduct>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking().Where(item => ids.Contains(item.Id)).ToListAsync(cancellationToken);
}
