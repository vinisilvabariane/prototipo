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

public sealed class FakePasswordResetNotifier(bool succeeds = true) : IPasswordResetNotifier
{
    public List<(User User, string Password)> Sent { get; } = [];

    public Task<bool> NotifyAsync(User user, string temporaryPassword, CancellationToken cancellationToken)
    {
        if (succeeds)
            Sent.Add((user, temporaryPassword));

        return Task.FromResult(succeeds);
    }
}
