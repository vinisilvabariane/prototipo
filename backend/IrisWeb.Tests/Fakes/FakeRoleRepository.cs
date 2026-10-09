using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeRoleRepository(FakeUserRepository? users = null)
    : InMemoryRepository<Role, ulong>, IRoleRepository
{
    public Task<bool> NameExistsAsync(string name, ulong? exceptRoleId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(role =>
            string.Equals(role.Name, name.Trim(), StringComparison.OrdinalIgnoreCase) && role.Id != exceptRoleId));

    // Igual ao repositório real: conta atribuição de usuário desativado também.
    public Task<bool> InUseAsync(ulong roleId, CancellationToken cancellationToken) =>
        Task.FromResult(users?.Stored.Any(user => user.HasRole(roleId)) ?? false);

    public Task<int> CountAsync(CancellationToken cancellationToken) => Task.FromResult(Items.Count);

    public Task<IReadOnlyList<Role>> ListPageAsync(int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Role>>([.. Items.OrderBy(role => role.Id).Skip(skip).Take(take)]);

    public Task<IReadOnlyList<Role>> ListOrderedAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Role>>([.. Items.OrderBy(role => role.Name)]);
}
