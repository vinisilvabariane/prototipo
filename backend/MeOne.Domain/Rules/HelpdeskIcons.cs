namespace MeOne.Domain.Rules;

public static class HelpdeskIcons
{
    public static IReadOnlyList<string> All { get; } =
    [
        "support",
        "build",
        "bolt",
        "memory",
        "inventory",
        "bug",
        "code",
        "science",
        "engineering",
        "settings",
        "report",
        "warning",
        "shipping",
        "handyman",
        "precision",
        "quality",
    ];

    public static bool Exists(string icon) => All.Contains(icon);
}
