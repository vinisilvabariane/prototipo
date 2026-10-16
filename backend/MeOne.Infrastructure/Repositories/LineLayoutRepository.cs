using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class LineLayoutRepository(MeOneDbContext context)
    : Repository<LineLayout, ulong>(context), ILineLayoutRepository
{
    public override Task<LineLayout?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        WithContent().FirstOrDefaultAsync(layout => layout.Id == id, cancellationToken);

    public override void Update(LineLayout entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<LineLayout?> FindAsync(ulong lineId, ulong productId, ulong shiftId, CancellationToken cancellationToken) =>
        WithContent().FirstOrDefaultAsync(
            layout => layout.LineId == lineId && layout.ProductId == productId && layout.ShiftId == shiftId,
            cancellationToken);

    public async Task<IReadOnlyList<LineLayout>> ListAsync(
        ulong? lineId,
        ulong? productId,
        ulong? shiftId,
        CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Include(layout => layout.Posts)
            .Where(layout => lineId == null || layout.LineId == lineId)
            .Where(layout => productId == null || layout.ProductId == productId)
            .Where(layout => shiftId == null || layout.ShiftId == shiftId)
            .ToListAsync(cancellationToken);

    private IQueryable<LineLayout> WithContent() =>
        DbSet.Include(layout => layout.Posts)
            .Include(layout => layout.Decorations)
            .AsSplitQuery();
}
