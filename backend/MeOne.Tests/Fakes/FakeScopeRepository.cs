using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeScopeRepository(FakeUserRepository? users = null)
    : InMemoryRepository<Scope, ulong>, IScopeRepository
{
    public Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptScopeId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(scope => scope.Code == normalizedCode && scope.Id != exceptScopeId));

    public Task<bool> HasChildrenAsync(ulong scopeId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(scope => scope.ParentId == scopeId));

    public Task<bool> InUseAsync(ulong scopeId, CancellationToken cancellationToken) =>
        Task.FromResult(users?.Stored.Any(user => user.Roles.Any(link => link.ScopeId == scopeId)) ?? false);

    public Task<IReadOnlyList<ulong>> GetChainAsync(ulong scopeId, CancellationToken cancellationToken)
    {
        var chain = new List<ulong>();
        var current = Items.FirstOrDefault(scope => scope.Id == scopeId);

        while (current is not null)
        {
            chain.Add(current.Id);
            current = Items.FirstOrDefault(scope => scope.Id == current.ParentId);
        }

        return Task.FromResult<IReadOnlyList<ulong>>(chain);
    }

    public Task<IReadOnlyList<Scope>> ListOrderedAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Scope>>([.. Items.OrderBy(scope => scope.Type).ThenBy(scope => scope.Code)]);
}
