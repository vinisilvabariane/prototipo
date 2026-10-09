using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Domain.Entities;

namespace IrisWeb.Application.UseCases.Stations;

internal static class StationMapping
{

    public const string PresenceKind = "presence";

    public const string VacancyKind = "vacancy";

    public const string UnknownKind = "unknown";

    public static StationTimelineEntryResponse ToEntry(StationPresence presence, DateTime openUntil) => new(
        presence.Id,
        PresenceKind,
        presence.Registration,
        presence.Workstation,
        presence.Line,
        presence.EnteredAt,
        presence.LeftAt,
        ElapsedSeconds(presence.EnteredAt, presence.LeftAt, presence.DurationSeconds, openUntil),
        presence.LeftAt is null);

    public static StationTimelineEntryResponse ToEntry(StationVacancy vacancy, DateTime openUntil) => new(
        vacancy.Id,
        VacancyKind,
        null,
        vacancy.Workstation,
        vacancy.Line,
        vacancy.StartedAt,
        vacancy.EndedAt,
        ElapsedSeconds(vacancy.StartedAt, vacancy.EndedAt, vacancy.DurationSeconds, openUntil),
        vacancy.EndedAt is null);

    public static StationTimelineEntryResponse ToEntry(StationUnknown unknown, DateTime openUntil) => new(
        unknown.Id,
        UnknownKind,
        null,
        unknown.Workstation,
        unknown.Line,
        unknown.StartedAt,
        unknown.EndedAt,
        ElapsedSeconds(unknown.StartedAt, unknown.EndedAt, unknown.DurationSeconds, openUntil),
        unknown.EndedAt is null,
        unknown.CandidateRegistration,
        unknown.CandidateDistance);

    public sealed record DayTotal(int OccupiedSeconds, int VacantSeconds, int PeopleCount, int EntryCount, int UnknownSeconds);

    public static IReadOnlyDictionary<DateOnly, DayTotal> DayTotals(
        IEnumerable<StationTimelineEntryResponse> entries) =>
        entries
            .GroupBy(entry => DateOnly.FromDateTime(entry.StartedAt))
            .ToDictionary(
                day => day.Key,
                day => new DayTotal(
                    SecondsOf(day, PresenceKind),
                    SecondsOf(day, VacancyKind),
                    day.Where(entry => entry.Registration is not null)
                        .Select(entry => entry.Registration)
                        .Distinct()
                        .Count(),
                    day.Count(),
                    SecondsOf(day, UnknownKind)));

    public static StationTimelineDayResponse ToDay(
        IGrouping<DateOnly, StationTimelineEntryResponse> day,
        DayTotal total) =>
        new(day.Key,
            total.OccupiedSeconds,
            total.VacantSeconds,
            total.PeopleCount,
            total.EntryCount,
            [.. day.OrderByDescending(entry => entry.StartedAt)],
            total.UnknownSeconds);

    private static int SecondsOf(IEnumerable<StationTimelineEntryResponse> entries, string kind) =>
        entries.Where(entry => entry.Kind == kind).Sum(entry => entry.DurationSeconds);

    private static int ElapsedSeconds(DateTime startedAt, DateTime? endedAt, int? stored, DateTime openUntil)
    {
        if (stored is > 0) return stored.Value;

        var closedAt = endedAt ?? openUntil;

        return (int)Math.Max(0, Math.Round((closedAt - startedAt).TotalSeconds));
    }
}
