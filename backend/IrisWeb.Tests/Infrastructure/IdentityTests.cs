using IrisWeb.Domain.Rules;
using IrisWeb.Infrastructure.Email;
using IrisWeb.Infrastructure.Identity;
using IrisWeb.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IrisWeb.Tests.Infrastructure;

public sealed class IdentityTests
{
    // Custo 12, igual ao Access Control: as senhas já gravadas em users continuam valendo.
    [Fact]
    public void BCrypt_IdaEVolta_EHashDeCusto12()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash("senha-forte");

        Assert.StartsWith("$2", hash);
        Assert.Contains("$12$", hash);
        Assert.True(hasher.Verify("senha-forte", hash));
        Assert.False(hasher.Verify("senha-errada", hash));
    }

    [Fact]
    public void BCrypt_HashNuloOuCorrompido_DevolveFalseSemLancar()
    {
        var hasher = new BCryptPasswordHasher();

        Assert.False(hasher.Verify("qualquer", null));
        Assert.False(hasher.Verify("qualquer", "nao-e-bcrypt"));
    }

    [Fact]
    public void RefreshToken_ValorCruAleatorio_HashDeterministico()
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
    public void SenhaProvisoria_TemPrefixoEUmDeCadaTipo()
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
    public void Email_TemplateELogoEstaoEmbutidosNaDll()
    {
        var names = typeof(SmtpPasswordResetNotifier).Assembly.GetManifestResourceNames();

        Assert.Contains("IrisWeb.Infrastructure.Email.Templates.ResetPassword.html", names);
        Assert.Contains("IrisWeb.Infrastructure.Email.Templates.logo-transparent.svg", names);
    }

    [Fact]
    public void Email_RenderEscapaHtmlDoNome()
    {
        var html = SmtpPasswordResetNotifier.Render(
            "<p>{{userName}}</p><img src=\"{{logoUrl}}\">",
            new Dictionary<string, string>
            {
                ["userName"] = "<script>x</script>",
                ["logoUrl"] = "cid:iris-logo",
            });

        Assert.Equal("<p>&lt;script&gt;x&lt;/script&gt;</p><img src=\"cid:iris-logo\">", html);
    }

    [Fact]
    public async Task Email_SemSmtpConfigurado_NaoLancaEDevolveFalse()
    {
        var f = new AccessFixture();
        var notifier = new SmtpPasswordResetNotifier(
            Options.Create(new EmailOptions()),
            NullLogger<SmtpPasswordResetNotifier>.Instance);

        var sent = await notifier.NotifyAsync(f.AddUser(12345), "Multi123", CancellationToken.None);

        Assert.False(sent);
    }
}
