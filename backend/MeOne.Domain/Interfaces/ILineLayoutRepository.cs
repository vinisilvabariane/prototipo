using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface ILineLayoutRepository : IRepository<LineLayout, ulong>
{
    Task<LineLayout?> FindAsync(ulong lineId, ulong productId, ulong shiftId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LineLayout>> ListAsync(ulong? lineId, ulong? productId, ulong? shiftId, CancellationToken cancellationToken);
}
