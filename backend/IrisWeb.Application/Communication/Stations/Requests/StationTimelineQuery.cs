namespace IrisWeb.Application.Communication.Stations.Requests;

public sealed record StationTimelineQuery
{
    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public string? Workstation { get; init; }

    public string? Line { get; init; }

    public string? Registration { get; init; }

    public int? Page { get; init; }

    public int? PageSize { get; init; }
}
