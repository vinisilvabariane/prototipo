using MeOne.Domain.Errors;

namespace MeOne.Domain.Rules;

public sealed record OeeFormulas(
    string Availability,
    string Performance,
    string Quality,
    string MachineOee,
    string LineAvailability,
    string LinePerformance,
    string LineQuality,
    string LineOee)
{
    public static OeeFormulas Default { get; } = new(
        "(elapsed_seconds - downtime_availability) / elapsed_seconds",
        "real_production / theoretical_production",
        "if(real_production > 0, (real_production - defects) / real_production, 1)",
        "availability * performance * quality",
        "availability_average",
        "performance_last_post",
        "quality_product",
        "availability * performance * quality");

    public static IReadOnlyList<(string Key, string Label, OeeScope Scope)> Slots { get; } =
    [
        ("availability", "disponibilidade do posto", OeeScope.PostPillar),
        ("performance", "desempenho do posto", OeeScope.PostPillar),
        ("quality", "qualidade do posto", OeeScope.PostPillar),
        ("machineOee", "OEE do posto", OeeScope.PostOee),
        ("lineAvailability", "disponibilidade da linha", OeeScope.LinePillar),
        ("linePerformance", "desempenho da linha", OeeScope.LinePillar),
        ("lineQuality", "qualidade da linha", OeeScope.LinePillar),
        ("lineOee", "OEE da linha", OeeScope.LineOee),
    ];

    public string this[string key] => key switch
    {
        "availability" => Availability,
        "performance" => Performance,
        "quality" => Quality,
        "machineOee" => MachineOee,
        "lineAvailability" => LineAvailability,
        "linePerformance" => LinePerformance,
        "lineQuality" => LineQuality,
        "lineOee" => LineOee,
        _ => string.Empty,
    };

    public OeeFormulas With(string key, string expression) => key switch
    {
        "availability" => this with { Availability = expression },
        "performance" => this with { Performance = expression },
        "quality" => this with { Quality = expression },
        "machineOee" => this with { MachineOee = expression },
        "lineAvailability" => this with { LineAvailability = expression },
        "linePerformance" => this with { LinePerformance = expression },
        "lineQuality" => this with { LineQuality = expression },
        "lineOee" => this with { LineOee = expression },
        _ => this,
    };

    public string? Check()
    {
        foreach (var (key, label, scope) in Slots)
        {
            var error = OeeFormula.Validate(this[key], OeeCatalog.AllowedIn(scope));

            if (error is not null)
                return OeeErrors.FormulaInvalid(label, error.Message);
        }

        return null;
    }

    public OeeFormulas Trimmed() => new(
        Availability.Trim(),
        Performance.Trim(),
        Quality.Trim(),
        MachineOee.Trim(),
        LineAvailability.Trim(),
        LinePerformance.Trim(),
        LineQuality.Trim(),
        LineOee.Trim());
}
