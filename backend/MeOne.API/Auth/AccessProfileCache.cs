using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Rules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace MeOne.API.Auth;

public sealed class AccessProfileCache(IMemoryCache cache, IServiceScopeFactory scopes) : IAccessProfileCache
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(30);

    private CancellationTokenSource _generation = new();

    public async Task<AccessProfile?> GetAsync(ulong userId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key(userId), out AccessProfile? cached))
            return cached;

        var generation = _generation.Token;

        using var scope = scopes.CreateScope();
        var result = await scope.ServiceProvider
            .GetRequiredService<ILoadAccessProfileUseCase>()
            .ExecuteAsync(userId, cancellationToken);

        var profile = result.IsSuccess ? result.Value : null;

        if (!generation.IsCancellationRequested)
        {
            cache.Set(Key(userId), profile, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(Lifetime)
                .AddExpirationToken(new CancellationChangeToken(generation)));
        }

        return profile;
    }

    public void Invalidate(ulong userId) => cache.Remove(Key(userId));

    public void InvalidateAll()
    {
        var previous = Interlocked.Exchange(ref _generation, new CancellationTokenSource());
        previous.Cancel();
        previous.Dispose();
    }

    private static string Key(ulong userId) => $"access-profile:{userId}";
}
