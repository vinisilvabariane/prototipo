using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IScopeRepository : IRepository<Scope, ulong>
{
    Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptScopeId, CancellationToken cancellationToken);

    Task<bool> HasChildrenAsync(ulong scopeId, CancellationToken cancellationToken);

    Task<bool> InUseAsync(ulong scopeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ulong>> GetChainAsync(ulong scopeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Scope>> ListOrderedAsync(CancellationToken cancellationToken);
}
