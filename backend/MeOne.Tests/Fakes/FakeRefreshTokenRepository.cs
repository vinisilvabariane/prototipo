using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeRefreshTokenRepository : InMemoryRepository<RefreshToken, ulong>, IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(token => token.TokenHash == tokenHash));

    public Task<int> RevokeAllForUserAsync(ulong userId, CancellationToken cancellationToken)
    {
        var active = Items.Where(token => token.UserId == userId && !token.Revoked).ToList();

        foreach (var token in active)
            token.Revoke();

        return Task.FromResult(active.Count);
    }
}
