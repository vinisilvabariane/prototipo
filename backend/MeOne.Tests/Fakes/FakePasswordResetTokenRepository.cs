using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakePasswordResetTokenRepository
    : InMemoryRepository<PasswordResetToken, ulong>, IPasswordResetTokenRepository
{
    public Task<PasswordResetToken?> GetLatestForUserAsync(ulong userId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Where(token => token.UserId == userId).MaxBy(token => token.Id));

    public Task<int> InvalidateAllForUserAsync(ulong userId, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var active = Items.Where(token => token.UserId == userId && token.UsedAt is null).ToList();

        foreach (var token in active)
            token.MarkUsed(nowUtc);

        return Task.FromResult(active.Count);
    }
}
