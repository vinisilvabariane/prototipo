using System.Security.Claims;
using MeOne.API.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace MeOne.Tests.API;

public sealed class HttpContextCurrentActorTests
{
    private static HttpContextCurrentActor ActorWith(params Claim[] claims)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
        };

        return new HttpContextCurrentActor(
            new HttpContextAccessor { HttpContext = context },
            new AccessProfileCache(new MemoryCache(new MemoryCacheOptions()), null!));
    }

    [Fact]
    public void Registration_RegistrationClaim_UsesAccessControlRegistration()
    {
        var actor = ActorWith(new Claim("registration", "123456"), new Claim("sub", "other-id"));

        Assert.Equal("123456", actor.Registration);
    }

    [Fact]
    public void Registration_OnlySub_FallsBackToSecondLink()
    {
        var actor = ActorWith(new Claim("sub", "654321"));

        Assert.Equal("654321", actor.Registration);
    }

    [Fact]
    public void Registration_OnlyNameClaim_FallsBackToThirdLink()
    {
        var actor = ActorWith(new Claim(ClaimTypes.Name, "jdoe"));

        Assert.Equal("jdoe", actor.Registration);
    }

    [Fact]
    public void Registration_AsJwtHandlerDelivers_FallsBackToNameIdentifier()
    {

        var actor = ActorWith(new Claim(ClaimTypes.NameIdentifier, "123456"));

        Assert.Equal("123456", actor.Registration);
    }

    [Fact]
    public void Registration_WithoutClaims_ReturnsNullInsteadOfUnknown()
    {
        var actor = ActorWith();

        Assert.Null(actor.Registration);
    }

    [Fact]
    public void UserId_ReadsSubOrNameIdentifierMappedByHandler()
    {
        Assert.Equal(42UL, ActorWith(new Claim("sub", "42")).UserId);
        Assert.Equal(42UL, ActorWith(new Claim(ClaimTypes.NameIdentifier, "42")).UserId);
    }

    [Fact]
    public void UserId_NonNumericSubOrAnonymous_ReturnsNull()
    {
        Assert.Null(ActorWith(new Claim("sub", "abc")).UserId);

        var anonymous = new HttpContextCurrentActor(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            new AccessProfileCache(new MemoryCache(new MemoryCacheOptions()), null!));
        Assert.Null(anonymous.UserId);
    }
}
