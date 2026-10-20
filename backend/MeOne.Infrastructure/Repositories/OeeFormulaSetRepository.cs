using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class OeeFormulaSetRepository(MeOneDbContext context)
    : Repository<OeeFormulaSet, ulong>(context), IOeeFormulaSetRepository
{
    public override void Update(OeeFormulaSet entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(item => item.Name == name && (exceptId == null || item.Id != exceptId), cancellationToken);

    public Task<OeeFormulaSet?> GetActiveAsync(CancellationToken cancellationToken) =>
        DbSet.AsNoTracking().FirstOrDefaultAsync(item => item.IsActive, cancellationToken);

    public async Task<IReadOnlyList<OeeFormulaSet>> ListAsync(CancellationToken cancellationToken) =>
        await DbSet.OrderBy(item => item.Name).ToListAsync(cancellationToken);
}
