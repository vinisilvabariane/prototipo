using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeRoleRepository(FakeUserRepository? users = null)
    : InMemoryRepository<Role, ulong>, IRoleRepository
{
    public Task<Role?> GetWithPermissionsAsync(ulong id, CancellationToken cancellationToken) =>
        GetByIdAsync(id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, ulong? exceptRoleId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(role =>
            string.Equals(role.Name, name.Trim(), StringComparison.OrdinalIgnoreCase) && role.Id != exceptRoleId));

    public Task<bool> InUseAsync(ulong roleId, CancellationToken cancellationToken) =>
        Task.FromResult(users?.Stored.Any(user => user.Roles.Any(link => link.RoleId == roleId)) ?? false);

    public Task<int> CountAsync(CancellationToken cancellationToken) => Task.FromResult(Items.Count);

    public Task<IReadOnlyList<Role>> ListPageAsync(int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Role>>([.. Items.OrderBy(role => role.Id).Skip(skip).Take(take)]);

    public Task<IReadOnlyList<Role>> ListOrderedAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Role>>([.. Items.OrderBy(role => role.Name)]);
}
