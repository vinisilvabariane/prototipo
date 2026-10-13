namespace MeOne.Domain.Rules;

public sealed record SessionPolicy(TimeSpan RefreshTokenLifetime)
{
    public static SessionPolicy Default { get; } = new(TimeSpan.FromHours(8));

    public DateTime RefreshExpiresAt(DateTime nowUtc) => nowUtc.Add(RefreshTokenLifetime);

    public bool IsCoherent => RefreshTokenLifetime > TimeSpan.Zero;
}
