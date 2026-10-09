namespace IrisWeb.Domain.Entities;

public sealed class StationVacancy : Entity<long>
{
    private StationVacancy() { }

    private StationVacancy(string workstation, string line, int startedSecondsAgo)
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

    public bool IsOpen => EndedAt is null;

    internal static StationVacancy Open(string workstation, string line, int startedSecondsAgo) =>
        new(workstation, line, startedSecondsAgo);

    public static StationVacancy Restore(
        long id,
        string workstation,
        string line,
        DateTime startedAt,
        DateTime? endedAt,
        int? durationSeconds) =>
        new()
        {
            Id = id,
            Workstation = workstation,
            Line = line,
            StartedAt = startedAt,
            EndedAt = endedAt,
            DurationSeconds = durationSeconds,
        };

    public bool Close(int endedSecondsAgo)
    {
        if (!IsOpen) return false;

        var endedAt = DateTime.Now.AddSeconds(-Math.Max(0, endedSecondsAgo));

        if (endedAt < StartedAt) endedAt = StartedAt;

        EndedAt = endedAt;
        DurationSeconds = (int)Math.Round((endedAt - StartedAt).TotalSeconds);

        return true;
    }
}
