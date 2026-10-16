using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class OrgPositionRepository(MeOneDbContext context)
    : Repository<OrgPosition, ulong>(context), IOrgPositionRepository
{
    public override void Update(OrgPosition entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> HasSubordinatesAsync(ulong positionId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(position => position.ParentId == positionId, cancellationToken);
}
