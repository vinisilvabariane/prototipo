using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken, ulong>
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<int> RevokeAllForUserAsync(ulong userId, CancellationToken cancellationToken);
}
