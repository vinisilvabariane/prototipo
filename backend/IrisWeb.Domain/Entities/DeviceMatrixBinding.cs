using IrisWeb.Domain.Rules;

namespace IrisWeb.Domain.Entities;

public sealed class DeviceMatrixBinding : Entity<string>
{
    private DeviceMatrixBinding() { }

    public string Line { get; private set; } = string.Empty;

    public string Product { get; private set; } = string.Empty;

    public string Shift { get; private set; } = string.Empty;

    public int StationId { get; private set; }

    public string StationName { get; private set; } = string.Empty;

    public DateTime UpdatedAt { get; private set; }

    public static DeviceMatrixBinding Create(
        string deviceId,
        string line,
        string product,
        string shift,
        int stationId,
        string? stationName)
    {
        var binding = new DeviceMatrixBinding { Id = deviceId.Trim() };
        binding.Rebind(line, product, shift, stationId, stationName);
        return binding;
    }

    public string PresenceWorkstation => StationNamingPolicy.NormalizeField($"#{StationId} {StationName}");

    public string PresenceLine => StationNamingPolicy.NormalizeField(Line);

    public StationPresence OpenPresence(string registration) =>
        StationPresence.Open(registration, PresenceWorkstation, PresenceLine);

    public StationVacancy OpenVacancy(int startedSecondsAgo) =>
        StationVacancy.Open(PresenceWorkstation, PresenceLine, startedSecondsAgo);

    public StationUnknown OpenUnknown(int startedSecondsAgo) =>
        StationUnknown.Open(PresenceWorkstation, PresenceLine, startedSecondsAgo);

    public void Rebind(string line, string product, string shift, int stationId, string? stationName)
    {
        Line = line.Trim();
        Product = product.Trim();
        Shift = shift.Trim();
        StationId = stationId;
        StationName = (stationName ?? string.Empty).Trim();
        UpdatedAt = DateTime.Now;
    }
}
