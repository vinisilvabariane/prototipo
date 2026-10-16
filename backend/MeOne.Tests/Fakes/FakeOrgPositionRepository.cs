using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeOrgPositionRepository : InMemoryRepository<OrgPosition, ulong>, IOrgPositionRepository
{
    public Task<bool> HasSubordinatesAsync(ulong positionId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(position => position.ParentId == positionId));
}
