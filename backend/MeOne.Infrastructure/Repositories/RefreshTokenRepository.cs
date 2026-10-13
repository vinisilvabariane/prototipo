using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(MeOneDbContext context)
    : Repository<RefreshToken, ulong>(context), IRefreshTokenRepository
{
    public override void Update(RefreshToken entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        DbSet.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public Task<int> RevokeAllForUserAsync(ulong userId, CancellationToken cancellationToken) =>
        DbSet.Where(token => token.UserId == userId && !token.Revoked)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.Revoked, true), cancellationToken);
}
