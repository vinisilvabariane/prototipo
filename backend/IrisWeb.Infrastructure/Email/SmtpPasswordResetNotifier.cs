using System.Globalization;
using System.Net;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace IrisWeb.Infrastructure.Email;

/// <summary>
/// E-mail de "senha resetada" via SMTP (MailKit). O template e o logo vão embutidos na DLL, então
/// não dependem de arquivo copiado para a pasta de publicação.
/// </summary>
public sealed class SmtpPasswordResetNotifier(
    IOptions<EmailOptions> options,
    ILogger<SmtpPasswordResetNotifier> logger) : IPasswordResetNotifier
{
    private const string Subject = "Senha resetada";
    private const string LogoContentId = "iris-logo";
    private const string TemplateResource = "IrisWeb.Infrastructure.Email.Templates.ResetPassword.html";
    private const string LogoResource = "IrisWeb.Infrastructure.Email.Templates.logo-transparent.svg";

    private static readonly CultureInfo PtBr = new("pt-BR");

    private static readonly Lazy<string> Template = new(() => ReadResourceText(TemplateResource));

    private readonly EmailOptions _options = options.Value;

    public async Task<bool> NotifyAsync(User user, string temporaryPassword, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return false;

        if (!_options.IsConfigured)
        {
            logger.LogWarning("Secao Email nao configurada; e-mail de reset da matricula {Registration} nao enviado.",
                user.Registration);
            return false;
        }

        try
        {
            using var message = BuildMessage(user, temporaryPassword);
            using var smtp = new SmtpClient { Timeout = _options.TimeoutSeconds * 1000 };

            await smtp.ConnectAsync(_options.SmtpServer, _options.Port, SecureSocketOptions.Auto, cancellationToken);
            await smtp.AuthenticateAsync(_options.User, _options.Password, cancellationToken);
            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Falha ao enviar o e-mail de reset da matricula {Registration}.",
                user.Registration);
            return false;
        }
    }

    private MimeMessage BuildMessage(User user, string temporaryPassword)
    {
        var html = Render(Template.Value, new Dictionary<string, string>
        {
            ["logoUrl"] = $"cid:{LogoContentId}",
            ["newPassword"] = temporaryPassword,
            ["registration"] = user.Registration.ToString(CultureInfo.InvariantCulture),
            ["resetAt"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", PtBr),
            ["userName"] = user.Name,
        });

        var body = new BodyBuilder { HtmlBody = html };

        var logo = body.LinkedResources.Add("logo.svg", ReadResourceBytes(LogoResource), new ContentType("image", "svg+xml"));
        logo.ContentId = LogoContentId;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(new MailboxAddress(user.Name, user.Email!));
        message.Subject = Subject;
        message.Body = body.ToMessageBody();

        return message;
    }

    /// <summary>Troca <c>{{chave}}</c> pelo valor, com HTML-encode — nome de usuário vai para dentro do HTML.</summary>
    public static string Render(string template, IReadOnlyDictionary<string, string> values) =>
        values.Aggregate(template, (html, pair) => html.Replace(
            $"{{{{{pair.Key}}}}}",
            pair.Key == "logoUrl" ? pair.Value : WebUtility.HtmlEncode(pair.Value),
            StringComparison.OrdinalIgnoreCase));

    private static string ReadResourceText(string name)
    {
        using var reader = new StreamReader(OpenResource(name));
        return reader.ReadToEnd();
    }

    private static byte[] ReadResourceBytes(string name)
    {
        using var stream = OpenResource(name);
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static Stream OpenResource(string name) =>
        typeof(SmtpPasswordResetNotifier).Assembly.GetManifestResourceStream(name)
        ?? throw new InvalidOperationException($"Recurso embutido {name} nao encontrado.");
}
