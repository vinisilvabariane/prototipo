using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class OeeFormulaSet : Entity<ulong>
{
    private OeeFormulaSet() { }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public string Availability { get; private set; } = string.Empty;

    public string Performance { get; private set; } = string.Empty;

    public string Quality { get; private set; } = string.Empty;

    public string MachineOee { get; private set; } = string.Empty;

    public string LineAvailability { get; private set; } = string.Empty;

    public string LinePerformance { get; private set; } = string.Empty;

    public string LineQuality { get; private set; } = string.Empty;

    public string LineOee { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ulong UpdatedBy { get; private set; }

    public OeeFormulas Formulas => new(Availability, Performance, Quality, MachineOee, LineAvailability, LinePerformance, LineQuality, LineOee);

    public static OeeFormulaSet Create(string name, OeeFormulas formulas, ulong userId, DateTime now)
    {
        var set = new OeeFormulaSet { CreatedAt = now };
        set.Apply(name, formulas, userId, now);
        return set;
    }

    public bool Update(string name, OeeFormulas formulas, ulong userId, DateTime now)
    {
        var trimmed = formulas.Trimmed();

        if (Name == NormalizeName(name) && Formulas == trimmed)
            return false;

        Apply(name, formulas, userId, now);
        return true;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public static string NormalizeName(string name) => name.Trim();

    private void Apply(string name, OeeFormulas formulas, ulong userId, DateTime now)
    {
        var trimmed = formulas.Trimmed();

        Name = NormalizeName(name);
        Availability = trimmed.Availability;
        Performance = trimmed.Performance;
        Quality = trimmed.Quality;
        MachineOee = trimmed.MachineOee;
        LineAvailability = trimmed.LineAvailability;
        LinePerformance = trimmed.LinePerformance;
        LineQuality = trimmed.LineQuality;
        LineOee = trimmed.LineOee;
        UpdatedBy = userId;
        UpdatedAt = now;
    }
}
