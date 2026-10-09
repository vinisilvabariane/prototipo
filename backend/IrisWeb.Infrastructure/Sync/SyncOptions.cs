namespace IrisWeb.Infrastructure.Sync;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;
}
