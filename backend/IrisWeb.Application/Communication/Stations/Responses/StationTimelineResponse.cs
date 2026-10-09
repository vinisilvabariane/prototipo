namespace IrisWeb.Application.Communication.Stations.Responses;

public sealed record StationTimelineEntryResponse(
    long Id,
    string Kind,
    string? Registration,
    string Workstation,
    string Line,
    DateTime StartedAt,
    DateTime? EndedAt,
    int DurationSeconds,
    bool Open,
    string? CandidateRegistration = null,
    float? CandidateDistance = null);

public sealed record StationTimelineDayResponse(
    DateOnly Date,
    int OccupiedSeconds,
    int VacantSeconds,
    int PeopleCount,
    int EntryCount,
    IReadOnlyList<StationTimelineEntryResponse> Entries,
    int UnknownSeconds = 0);

public sealed record StationTimelineResponse(
    DateTime From,
    DateTime To,
    int Page,
    int PageSize,
    int EntryCount,
    int TotalPages,
    int DayCount,
    int OccupiedSeconds,
    int VacantSeconds,
    int PeopleCount,
    bool Truncated,
    IReadOnlyList<StationTimelineDayResponse> Days,
    int UnknownSeconds = 0);
