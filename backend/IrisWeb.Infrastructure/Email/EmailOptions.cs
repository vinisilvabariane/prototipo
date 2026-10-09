namespace IrisWeb.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpServer { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "IrisWeb";

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SmtpServer) &&
        Port > 0 &&
        !string.IsNullOrWhiteSpace(FromAddress) &&
        !string.IsNullOrWhiteSpace(User) &&
        !string.IsNullOrWhiteSpace(Password);
}
