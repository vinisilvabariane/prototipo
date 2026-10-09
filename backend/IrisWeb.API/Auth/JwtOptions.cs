namespace IrisWeb.API.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HS256 exige chave de pelo menos 256 bits (32 bytes em UTF-8).</summary>
    public const int MinKeyBytes = 32;

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "access-control";

    /// <summary>Todas vão no claim <c>aud</c> do token emitido e são as únicas aceitas na validação.</summary>
    public string[] Audiences { get; set; } = [];

    public int ExpirationInMinutes { get; set; } = 15;

    /// <summary>Lido pela Infrastructure para montar a <c>SessionPolicy</c>.</summary>
    public double RefreshTokenExpirationInHours { get; set; } = 8;
}
