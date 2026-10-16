namespace MeOne.Domain.Entities;

public sealed class ProductionShift : Entity<ulong>
{
    private ProductionShift() { }

    public string Name { get; private set; } = string.Empty;

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public bool Active { get; private set; } = true;

    public static ProductionShift Create(string name, TimeOnly startTime, TimeOnly endTime) => new()
    {
        Name = NormalizeName(name),
        StartTime = startTime,
        EndTime = endTime,
    };

    public bool Update(string name, TimeOnly startTime, TimeOnly endTime, bool active)
    {
        var normalized = NormalizeName(name);

        if (Name == normalized && StartTime == startTime && EndTime == endTime && Active == active)
            return false;

        Name = normalized;
        StartTime = startTime;
        EndTime = endTime;
        Active = active;
        return true;
    }

    public static string NormalizeName(string name) => name.Trim();
}
