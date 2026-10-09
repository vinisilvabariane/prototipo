using System.Globalization;
using System.Text;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IrisWeb.API.Auth;

/// <summary>
/// Emite o JWT com os mesmos claims do Access Control (<c>registration</c>, <c>department</c>,
/// <c>roles</c>, <c>applications</c>), para Q-Zero e master-data continuarem aceitando o token —
/// desde que validem com a mesma chave, issuer e audiences.
/// </summary>
public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private const string DepartmentFallback = "not found";

    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtAccessTokenIssuer(JwtOptions options)
    {
        _options = options;
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
            SecurityAlgorithms.HmacSha256);
    }

    public string Issue(User user)
    {
        var now = DateTime.UtcNow;

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(CultureInfo.InvariantCulture),
            [JwtRegisteredClaimNames.Email] = user.Email ?? string.Empty,
            [JwtRegisteredClaimNames.Name] = user.Name,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [JwtRegisteredClaimNames.Aud] = _options.Audiences,
            ["registration"] = user.Registration.ToString(CultureInfo.InvariantCulture),
            ["department"] = user.Department?.Name ?? DepartmentFallback,
            ["roles"] = user.RoleNames.ToArray(),
            ["applications"] = user.ApplicationNames.ToArray(),
        };

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Claims = claims,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_options.ExpirationInMinutes),
            SigningCredentials = _credentials,
        });
    }
}
