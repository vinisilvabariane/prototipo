using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IUserRepository : IRepository<User, ulong>
{
    Task<User?> GetByRegistrationAsync(uint registration, CancellationToken cancellationToken);

    Task<User?> GetWithAccessAsync(ulong id, CancellationToken cancellationToken);

    Task<User?> GetWithAccessByRegistrationAsync(uint registration, CancellationToken cancellationToken);

    Task<bool> RegistrationExistsAsync(uint registration, ulong? exceptUserId, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, ulong? exceptUserId, CancellationToken cancellationToken);

    Task<int> CountAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> SearchAsync(string? search, int skip, int take, CancellationToken cancellationToken);

    Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> ListActiveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> GetActiveByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> GetByIdsIncludingInactiveAsync(
        IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);
}
