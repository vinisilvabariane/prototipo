using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IHelpdeskSectorRepository : IRepository<HelpdeskSector, ulong>
{
    Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpdeskSector>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpdeskSector>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken);
}
