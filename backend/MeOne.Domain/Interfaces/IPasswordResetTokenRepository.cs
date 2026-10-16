using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken, ulong>
{
    Task<PasswordResetToken?> GetLatestForUserAsync(ulong userId, CancellationToken cancellationToken);

    Task<int> InvalidateAllForUserAsync(ulong userId, DateTime nowUtc, CancellationToken cancellationToken);
}
