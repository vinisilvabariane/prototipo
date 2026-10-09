using System.Text.RegularExpressions;

namespace IrisWeb.Domain.Rules;

public static partial class StationNamingPolicy
{

    public const string Default = "default";

    public const int MaxFieldLength = 64;

    public const int MaxRegistrationLength = 32;

    public static string Normalize(string? raw)
    {
        var trimmed = Collapse(raw);

        return trimmed.Length == 0 ? Default : trimmed.ToUpperInvariant();
    }

    /// <summary>
    /// Le o rotulo que a presenca grava ("#14 PRENSA BATERIA") de volta em id e nome do posto
    /// da matriz. Rotulo fora desse formato (linhas antigas, do posto Iris) volta nulo.
    /// </summary>
    public static (int StationId, string Name)? ParseWorkstation(string? workstation)
    {
        var match = WorkstationRegex().Match(workstation ?? string.Empty);
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var id)) return null;
        return (id, match.Groups[2].Value.Trim());
    }

    [GeneratedRegex(@"^#(\d+)\s+(.*)$")]
    private static partial Regex WorkstationRegex();

    public static string NormalizeField(string? raw)
    {
        var normalized = Normalize(raw);

        return normalized.Length <= MaxFieldLength ? normalized : normalized[..MaxFieldLength].TrimEnd();
    }

    private static string Collapse(string? raw) =>
        raw is null ? string.Empty : WhitespaceRegex().Replace(raw, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
