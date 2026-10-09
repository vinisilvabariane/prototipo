namespace IrisWeb.Domain.Entities;

public sealed class StationPresence : Entity<long>
{
    private StationPresence() { }

    private StationPresence(string registration, string workstation, string line)
    {
        Registration = registration;
        Workstation = workstation;
        Line = line;
        EnteredAt = DateTime.Now;
    }

    public string Registration { get; private set; } = string.Empty;

    public string Workstation { get; private set; } = string.Empty;

    public string Line { get; private set; } = string.Empty;

    public DateTime EnteredAt { get; private set; }

    public DateTime? LeftAt { get; private set; }

    public int? DurationSeconds { get; private set; }

    /// <summary>Nome do operador no Sync quando a presenca abriu.</summary>
    public string? CollaboratorName { get; private set; }

    /// <summary>
    /// Nivel do operador na atividade do posto quando a presenca abriu, como o Sync mostra
    /// ("Qualificado", "Sem treinamento"...). Congelado aqui: o nivel muda com o tempo e o
    /// Sync nao devolve o nivel historico pela API.
    /// </summary>
    public string? TrainingLevel { get; private set; }

    /// <summary>Quem o Sync dizia estar alocado no posto quando a presenca abriu.</summary>
    public string? AllocatedRegistration { get; private set; }

    public bool IsOpen => LeftAt is null;

    internal static StationPresence Open(string registration, string workstation, string line) =>
        new(registration, workstation, line);

    public static StationPresence Restore(
        long id,
        string registration,
        string workstation,
        string line,
        DateTime enteredAt,
        DateTime? leftAt,
        int? durationSeconds,
        string? collaboratorName = null,
        string? trainingLevel = null,
        string? allocatedRegistration = null) =>
        new()
        {
            Id = id,
            Registration = registration,
            Workstation = workstation,
            Line = line,
            EnteredAt = enteredAt,
            LeftAt = leftAt,
            DurationSeconds = durationSeconds,
            CollaboratorName = collaboratorName,
            TrainingLevel = trainingLevel,
            AllocatedRegistration = allocatedRegistration,
        };

    public void Describe(string? collaboratorName, string? trainingLevel, string? allocatedRegistration)
    {
        CollaboratorName = Clip(collaboratorName, MaxNameLength);
        TrainingLevel = Clip(trainingLevel, MaxLevelLength);
        AllocatedRegistration = Clip(allocatedRegistration, MaxRegistrationLength);
    }

    public const int MaxNameLength = 150;

    public const int MaxLevelLength = 64;

    public const int MaxRegistrationLength = 32;

    private static string? Clip(string? value, int max)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    public bool Close(int graceSeconds)
    {
        if (!IsOpen) return false;

        var leftAt = DateTime.Now.AddSeconds(-Math.Max(0, graceSeconds));

        if (leftAt < EnteredAt) leftAt = EnteredAt;

        LeftAt = leftAt;
        DurationSeconds = (int)Math.Round((leftAt - EnteredAt).TotalSeconds);

        return true;
    }
}
