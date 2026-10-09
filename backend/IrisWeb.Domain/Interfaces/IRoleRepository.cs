using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role, ulong>
{
    Task<bool> NameExistsAsync(string name, ulong? exceptRoleId, CancellationToken cancellationToken);

    Task<bool> InUseAsync(ulong roleId, CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Role>> ListPageAsync(int skip, int take, CancellationToken cancellationToken);

    Task<IReadOnlyList<Role>> ListOrderedAsync(CancellationToken cancellationToken);
}
