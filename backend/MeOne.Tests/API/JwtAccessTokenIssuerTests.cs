using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MeOne.API.Auth;
using MeOne.Domain.Entities;
using MeOne.Tests.Fakes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MeOne.Tests.API;

public sealed class JwtAccessTokenIssuerTests
{
    private static readonly JwtOptions Options = new()
    {
        Key = "test-key-with-more-than-32-bytes!!!",
        Issuer = "access-control",
        Audiences = ["q-zero", "master-data"],
        ExpirationInMinutes = 15,
    };

    private static User UserWithAccess()
    {
        var f = new AccessFixture();
        return f.AddUser(12345, roles: [f.MasterRole, f.OperatorRole]);
    }

    private static TokenValidationParameters SameValidationAsProgram() => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Options.Issuer,
        ValidateAudience = true,
        ValidAudiences = Options.Audiences,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.Key)),
    };

    [Fact]
    public async Task IssuedToken_PassesApiOwnValidation()
    {
        var token = new JwtAccessTokenIssuer(Options).Issue(UserWithAccess());

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, SameValidationAsProgram());

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task IssuedToken_WithAnotherKey_IsRejected()
    {
        var otherOptions = new JwtOptions
        {
            Key = "another-key-with-more-than-32-bytes!!",
            Issuer = Options.Issuer,
            Audiences = Options.Audiences,
        };

        var token = new JwtAccessTokenIssuer(otherOptions).Issue(UserWithAccess());

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, SameValidationAsProgram());

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Claims_KeepTheAccessControlContract()
    {
        var user = UserWithAccess();

        var jwt = new JsonWebToken(new JwtAccessTokenIssuer(Options).Issue(user));
        using var payload = JsonDocument.Parse(Base64UrlEncoder.Decode(jwt.EncodedPayload));
        var root = payload.RootElement;

        Assert.Equal(user.Id.ToString(), root.GetProperty("sub").GetString());
        Assert.Equal("12345", root.GetProperty("registration").GetString());
        Assert.Equal("ti", root.GetProperty("department").GetString());
        Assert.Equal("John Doe Smith", root.GetProperty("name").GetString());
        Assert.Equal("access-control", root.GetProperty("iss").GetString());

        Assert.Equal(JsonValueKind.Array, root.GetProperty("roles").ValueKind);
        Assert.Equal(["master", "operator"], root.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
        Assert.Equal(JsonValueKind.Array, root.GetProperty("applications").ValueKind);
        Assert.Equal(["q-zero", "master-data"], root.GetProperty("aud").EnumerateArray().Select(x => x.GetString()));
    }

    [Fact]
    public async Task TokenRoles_BecomeRoleClaims_SoAuthorizeWorks()
    {
        var token = new JwtAccessTokenIssuer(Options).Issue(UserWithAccess());
        var validated = await new JsonWebTokenHandler().ValidateTokenAsync(token, SameValidationAsProgram());

        var principal = new ClaimsPrincipal(validated.ClaimsIdentity);
        await new RolesClaimsTransformation().TransformAsync(principal);

        Assert.True(principal.IsInRole(AuthPolicies.MasterRole));
        Assert.True(principal.IsInRole("operator"));
    }
}
