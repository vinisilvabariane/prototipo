using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class HelpdeskFieldTemplateRepository(MeOneDbContext context)
    : Repository<HelpdeskFieldTemplate, ulong>(context), IHelpdeskFieldTemplateRepository
{
    public override void Update(HelpdeskFieldTemplate entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(item => item.Name == name && (exceptId == null || item.Id != exceptId), cancellationToken);

    public async Task<IReadOnlyList<HelpdeskFieldTemplate>> ListAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking().OrderBy(item => item.Name).ToListAsync(cancellationToken);
}
