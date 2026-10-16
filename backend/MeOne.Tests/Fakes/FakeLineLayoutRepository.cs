using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeLineLayoutRepository : InMemoryRepository<LineLayout, ulong>, ILineLayoutRepository
{
    private ulong _lastChildIdentity;

    public Task<LineLayout?> FindAsync(ulong lineId, ulong productId, ulong shiftId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(layout => layout.LineId == lineId && layout.ProductId == productId && layout.ShiftId == shiftId));

    public Task<IReadOnlyList<LineLayout>> ListAsync(ulong? lineId, ulong? productId, ulong? shiftId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<LineLayout>>(
        [
            .. Items
                .Where(layout => lineId is null || layout.LineId == lineId)
                .Where(layout => productId is null || layout.ProductId == productId)
                .Where(layout => shiftId is null || layout.ShiftId == shiftId),
        ]);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var layout in Items)
        {
            foreach (var post in layout.Posts.Where(item => item.Id == 0))
                post.Id = ++_lastChildIdentity;

            foreach (var decoration in layout.Decorations.Where(item => item.Id == 0))
                decoration.Id = ++_lastChildIdentity;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
