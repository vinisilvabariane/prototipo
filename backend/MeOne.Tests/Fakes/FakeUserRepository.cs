using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeUserRepository : InMemoryRepository<User, ulong>, IUserRepository
{
    private IEnumerable<User> Active => Items.Where(user => user.Active);

    public override Task<User?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        Task.FromResult(Active.FirstOrDefault(user => user.Id == id));

    public Task<User?> GetByRegistrationAsync(uint registration, CancellationToken cancellationToken) =>
        Task.FromResult(Active.FirstOrDefault(user => user.Registration == registration));

    public Task<User?> GetWithAccessAsync(ulong id, CancellationToken cancellationToken) =>
        GetByIdAsync(id, cancellationToken);

    public Task<User?> GetWithAccessByRegistrationAsync(uint registration, CancellationToken cancellationToken) =>
        GetByRegistrationAsync(registration, cancellationToken);

    public Task<bool> RegistrationExistsAsync(uint registration, ulong? exceptUserId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(user => user.Registration == registration && user.Id != exceptUserId));

    public Task<bool> EmailExistsAsync(string email, ulong? exceptUserId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(user => user.Email == email && user.Id != exceptUserId));

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) =>
        Task.FromResult(Matching(search).Count());

    public Task<IReadOnlyList<User>> SearchAsync(string? search, int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<User>>([.. Matching(search).OrderBy(user => user.Id).Skip(skip).Take(take)]);

    private IEnumerable<User> Matching(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return Active;

        var term = search.Trim();

        return Active.Where(user =>
            user.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (user.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
            user.Registration.ToString() == term);
    }
}
