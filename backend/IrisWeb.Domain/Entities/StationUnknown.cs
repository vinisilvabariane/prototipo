namespace IrisWeb.Domain.Entities;

public sealed class StationUnknown : Entity<long>
{
    private StationUnknown() { }

    private StationUnknown(string workstation, string line, int startedSecondsAgo)
    {
        Workstation = workstation;
        Line = line;
        StartedAt = DateTime.Now.AddSeconds(-Math.Max(0, startedSecondsAgo));
    }

    public string Workstation { get; private set; } = string.Empty;

    public string Line { get; private set; } = string.Empty;

    public DateTime StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public int? DurationSeconds { get; private set; }

    public string? CandidateRegistration { get; private set; }

    public float? CandidateDistance { get; private set; }

    public bool IsOpen => EndedAt is null;

    internal static StationUnknown Open(string workstation, string line, int startedSecondsAgo) =>
        new(workstation, line, startedSecondsAgo);

    public static StationUnknown Restore(
        long id,
        string workstation,
        string line,
        DateTime startedAt,
        DateTime? endedAt,
        int? durationSeconds,
        string? candidateRegistration,
        float? candidateDistance) =>
        new()
        {
            Id = id,
            Workstation = workstation,
            Line = line,
            StartedAt = startedAt,
            EndedAt = endedAt,
            DurationSeconds = durationSeconds,
            CandidateRegistration = candidateRegistration,
            CandidateDistance = candidateDistance,
        };

    public bool Close(int endedSecondsAgo, string? candidateRegistration, float? candidateDistance)
    {
        if (!IsOpen) return false;

        var endedAt = DateTime.Now.AddSeconds(-Math.Max(0, endedSecondsAgo));

        if (endedAt < StartedAt) endedAt = StartedAt;

        EndedAt = endedAt;
        DurationSeconds = (int)Math.Round((endedAt - StartedAt).TotalSeconds);

        var candidate = candidateRegistration?.Trim();
        if (!string.IsNullOrEmpty(candidate) && candidateDistance is >= 0)
        {
            CandidateRegistration = candidate;
            CandidateDistance = candidateDistance;
        }

        return true;
    }
}
