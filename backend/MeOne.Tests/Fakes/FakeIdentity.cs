using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Entities;

namespace MeOne.Tests.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    public int VerifyCalls { get; private set; }

    public string Hash(string password) => $"hash:{password}";

    public bool Verify(string password, string? hash)
    {
        VerifyCalls++;
        return hash == Hash(password);
    }
}

public sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
{
    private int _sequence;

    public string Generate() => $"raw-{++_sequence}";

    public string Hash(string token) => $"sha:{token}";
}

public sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
{
    public List<User> IssuedFor { get; } = [];

    public string Issue(User user)
    {
        IssuedFor.Add(user);
        return $"jwt-{user.Id}-[{string.Join(",", user.RoleNames)}]";
    }
}

public sealed class FakePasswordResetNotifier : IPasswordResetNotifier
{
    public List<(User User, string Code)> Sent { get; } = [];

    public Task<bool> SendResetCodeAsync(User user, string code, DateTime expiresAtUtc, CancellationToken cancellationToken)
    {
        Sent.Add((user, code));
        return Task.FromResult(true);
    }
}

public sealed class FakeAccessProfileCache : IAccessProfileCache
{
    public List<ulong> Invalidated { get; } = [];

    public int InvalidatedAll { get; private set; }

    public void Invalidate(ulong userId) => Invalidated.Add(userId);

    public void InvalidateAll() => InvalidatedAll++;
}
