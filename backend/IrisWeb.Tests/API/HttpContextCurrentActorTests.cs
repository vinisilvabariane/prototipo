using System.Security.Claims;
using IrisWeb.API.Auth;
using Microsoft.AspNetCore.Http;

namespace IrisWeb.Tests.API;

public sealed class HttpContextCurrentActorTests
{
    private static HttpContextCurrentActor AtorCom(params Claim[] claims)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
        };

        return new HttpContextCurrentActor(new HttpContextAccessor { HttpContext = context });
    }

    [Fact]
    public void Registration_ClaimRegistration_UsaAMatriculaDoAccessControl()
    {
        var actor = AtorCom(new Claim("registration", "123456"), new Claim("sub", "outro-id"));

        Assert.Equal("123456", actor.Registration);
    }

    [Fact]
    public void Registration_SomenteSub_CaiNoSegundoEloDaCadeia()
    {
        var actor = AtorCom(new Claim("sub", "654321"));

        Assert.Equal("654321", actor.Registration);
    }

    [Fact]
    public void Registration_SomenteClaimDeNome_CaiNoTerceiroEloDaCadeia()
    {
        var actor = AtorCom(new Claim(ClaimTypes.Name, "vinicius"));

        Assert.Equal("vinicius", actor.Registration);
    }

    [Fact]
    public void Registration_ComoOHandlerJwtEntrega_CaiNoNameIdentifier()
    {

        var actor = AtorCom(new Claim(ClaimTypes.NameIdentifier, "123456"));

        Assert.Equal("123456", actor.Registration);
    }

    [Fact]
    public void Registration_SemNenhumClaim_DevolveNullEmVezDeUnknown()
    {
        var actor = AtorCom();

        Assert.Null(actor.Registration);
    }

    [Fact]
    public void UserId_LeOSubOuONameIdentifierMapeadoPeloHandler()
    {
        Assert.Equal(42UL, AtorCom(new Claim("sub", "42")).UserId);
        Assert.Equal(42UL, AtorCom(new Claim(ClaimTypes.NameIdentifier, "42")).UserId);
    }

    [Fact]
    public void UserId_SubNaoNumericoOuAnonimo_DevolveNull()
    {
        Assert.Null(AtorCom(new Claim("sub", "abc")).UserId);

        var anonimo = new HttpContextCurrentActor(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        Assert.Null(anonimo.UserId);
    }

    [Fact]
    public void IsInRole_UsaOsClaimsDeRole()
    {
        var actor = AtorCom(new Claim(ClaimTypes.Role, "master"));

        Assert.True(actor.IsInRole("master"));
        Assert.False(actor.IsInRole("operador"));
    }
}
