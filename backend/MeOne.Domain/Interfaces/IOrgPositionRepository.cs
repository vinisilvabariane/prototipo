using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IOrgPositionRepository : IRepository<OrgPosition, ulong>
{
    Task<bool> HasSubordinatesAsync(ulong positionId, CancellationToken cancellationToken);
}
