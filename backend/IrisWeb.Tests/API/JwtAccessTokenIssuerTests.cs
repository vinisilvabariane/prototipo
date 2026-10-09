using System.Security.Claims;
using System.Text;
using System.Text.Json;
using IrisWeb.API.Auth;
using IrisWeb.Domain.Entities;
using IrisWeb.Tests.Fakes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IrisWeb.Tests.API;

public sealed class JwtAccessTokenIssuerTests
{
    private static readonly JwtOptions Options = new()
    {
        Key = "chave-de-teste-com-mais-de-32-bytes!!",
        Issuer = "access-control",
        Audiences = ["q-zero", "master-data"],
        ExpirationInMinutes = 15,
    };

    private static User UsuarioComAcesso()
    {
        var f = new AccessFixture();
        return f.AddUser(12345, roles: [f.MasterRole, f.OperatorRole]);
    }

    private static TokenValidationParameters MesmaValidacaoDoProgram() => new()
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
    public async Task TokenEmitido_PassaNaValidacaoDaPropriaApi()
    {
        var token = new JwtAccessTokenIssuer(Options).Issue(UsuarioComAcesso());

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, MesmaValidacaoDoProgram());

        Assert.True(result.IsValid, result.Exception?.Message);
    }

    [Fact]
    public async Task TokenEmitido_ComOutraChave_ERecusado()
    {
        var outra = new JwtOptions
        {
            Key = "outra-chave-qualquer-com-mais-de-32-bytes",
            Issuer = Options.Issuer,
            Audiences = Options.Audiences,
        };

        var token = new JwtAccessTokenIssuer(outra).Issue(UsuarioComAcesso());

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, MesmaValidacaoDoProgram());

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Claims_MantemOContratoDoAccessControl()
    {
        var user = UsuarioComAcesso();

        var jwt = new JsonWebToken(new JwtAccessTokenIssuer(Options).Issue(user));
        using var payload = JsonDocument.Parse(Base64UrlEncoder.Decode(jwt.EncodedPayload));
        var root = payload.RootElement;

        Assert.Equal(user.Id.ToString(), root.GetProperty("sub").GetString());
        Assert.Equal("12345", root.GetProperty("registration").GetString());
        Assert.Equal("ti", root.GetProperty("department").GetString());
        Assert.Equal("Fulano de Tal", root.GetProperty("name").GetString());
        Assert.Equal("access-control", root.GetProperty("iss").GetString());

        Assert.Equal(JsonValueKind.Array, root.GetProperty("roles").ValueKind);
        Assert.Equal(["master", "operador"], root.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
        Assert.Equal(JsonValueKind.Array, root.GetProperty("applications").ValueKind);
        Assert.Equal(["q-zero", "master-data"], root.GetProperty("aud").EnumerateArray().Select(x => x.GetString()));
    }

    [Fact]
    public async Task RolesDoToken_ViramClaimTypesRole_ParaOAuthorizeFuncionar()
    {
        var token = new JwtAccessTokenIssuer(Options).Issue(UsuarioComAcesso());
        var validated = await new JsonWebTokenHandler().ValidateTokenAsync(token, MesmaValidacaoDoProgram());

        var principal = new ClaimsPrincipal(validated.ClaimsIdentity);
        await new RolesClaimsTransformation().TransformAsync(principal);

        Assert.True(principal.IsInRole(AuthPolicies.MasterRole));
        Assert.True(principal.IsInRole("operador"));
    }
}
