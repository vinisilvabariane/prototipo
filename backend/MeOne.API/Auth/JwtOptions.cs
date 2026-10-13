namespace MeOne.API.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public const int MinKeyBytes = 32;

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "access-control";

    public string[] Audiences { get; set; } = [];

    public int ExpirationInMinutes { get; set; } = 15;

    public double RefreshTokenExpirationInHours { get; set; } = 8;
}
