using System.Globalization;
using System.Text;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MeOne.API.Auth;

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
            [TokenClaims.Registration] = user.Registration.ToString(CultureInfo.InvariantCulture),
            [TokenClaims.SecurityStamp] = user.SecurityStamp,
            ["department"] = user.Department?.Name ?? DepartmentFallback,
            [TokenClaims.Roles] = user.RoleNames.ToArray(),
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
