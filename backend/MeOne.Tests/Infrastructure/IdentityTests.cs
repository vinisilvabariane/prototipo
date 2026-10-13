using MeOne.Domain.Rules;
using MeOne.Infrastructure.Email;
using MeOne.Infrastructure.Identity;
using MeOne.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace MeOne.Tests.Infrastructure;

public sealed class IdentityTests
{
    [Fact]
    public void BCrypt_RoundTrip_UsesCost12Hash()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash("strong-password");

        Assert.StartsWith("$2", hash);
        Assert.Contains("$12$", hash);
        Assert.True(hasher.Verify("strong-password", hash));
        Assert.False(hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void BCrypt_NullOrCorruptedHash_ReturnsFalseWithoutThrowing()
    {
        var hasher = new BCryptPasswordHasher();

        Assert.False(hasher.Verify("anything", null));
        Assert.False(hasher.Verify("anything", "not-bcrypt"));
    }

    [Fact]
    public void RefreshToken_RandomRawValue_DeterministicHash()
    {
        var generator = new RefreshTokenGenerator();

        var a = generator.Generate();
        var b = generator.Generate();

        Assert.NotEqual(a, b);
        Assert.Equal(generator.Hash(a), generator.Hash(a));
        Assert.NotEqual(a, generator.Hash(a));
        Assert.True(generator.Hash(a).Length <= 255);
    }

    [Fact]
    public void TemporaryPassword_HasPrefixAndOneOfEachKind()
    {
        for (var i = 0; i < 200; i++)
        {
            var password = TemporaryPasswordPolicy.Generate();
            var random = password[TemporaryPasswordPolicy.Prefix.Length..];

            Assert.StartsWith(TemporaryPasswordPolicy.Prefix, password);
            Assert.Equal(6, random.Length);
            Assert.Contains(random, char.IsUpper);
            Assert.Contains(random, char.IsLower);
            Assert.Contains(random, char.IsDigit);
        }
    }

    [Fact]
    public void Email_TemplateAndLogoAreEmbeddedInDll()
    {
        var names = typeof(SmtpPasswordResetNotifier).Assembly.GetManifestResourceNames();

        Assert.Contains("MeOne.Infrastructure.Email.Templates.ResetPassword.html", names);
        Assert.Contains("MeOne.Infrastructure.Email.Templates.logo-transparent.svg", names);
    }

    [Fact]
    public void Email_RenderEscapesHtmlInName()
    {
        var html = SmtpPasswordResetNotifier.Render(
            "<p>{{userName}}</p><img src=\"{{logoUrl}}\">",
            new Dictionary<string, string>
            {
                ["userName"] = "<script>x</script>",
                ["logoUrl"] = "cid:meone-logo",
            });

        Assert.Equal("<p>&lt;script&gt;x&lt;/script&gt;</p><img src=\"cid:meone-logo\">", html);
    }

    [Fact]
    public async Task Email_WithoutSmtpConfigured_DoesNotThrowAndReturnsFalse()
    {
        var f = new AccessFixture();
        var notifier = new SmtpPasswordResetNotifier(
            Options.Create(new EmailOptions()),
            NullLogger<SmtpPasswordResetNotifier>.Instance);

        var sent = await notifier.NotifyAsync(f.AddUser(12345), "Multi123", CancellationToken.None);

        Assert.False(sent);
    }
}
