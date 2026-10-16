using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class PasswordResetTokenRepository(MeOneDbContext context)
    : Repository<PasswordResetToken, ulong>(context), IPasswordResetTokenRepository
{
    public override void Update(PasswordResetToken entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<PasswordResetToken?> GetLatestForUserAsync(ulong userId, CancellationToken cancellationToken) =>
        DbSet.Where(token => token.UserId == userId)
            .OrderByDescending(token => token.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int> InvalidateAllForUserAsync(ulong userId, DateTime nowUtc, CancellationToken cancellationToken) =>
        DbSet.Where(token => token.UserId == userId && token.UsedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.UsedAt, nowUtc), cancellationToken);
}
