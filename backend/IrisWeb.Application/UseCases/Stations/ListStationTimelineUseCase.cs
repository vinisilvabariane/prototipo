using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Stations;

public sealed class ListStationTimelineUseCase(
    IStationPresenceRepository presences,
    IStationVacancyRepository vacancies,
    IStationUnknownRepository unknowns) : IListStationTimelineUseCase
{
    public const int MaxRangeDays = 31;

    public const int EntryLimit = 5_000;

    public const int DefaultPageSize = 100;

    public const int MaxPageSize = 500;

    public async Task<Result<StationTimelineResponse>> ExecuteAsync(
        StationTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var (from, to) = ResolveWindow(query, now);
        var openUntil = now < to ? now : to;

        var workstation = Trimmed(query.Workstation);
        var line = Trimmed(query.Line);
        var registration = Trimmed(query.Registration);

        var foundPresences = await presences.ListOverlappingAsync(
            from, to, workstation, line, registration, EntryLimit, cancellationToken);

        var foundVacancies = registration is null
            ? await vacancies.ListOverlappingAsync(from, to, workstation, line, EntryLimit, cancellationToken)
            : [];

        var foundUnknowns = registration is null
            ? await unknowns.ListOverlappingAsync(from, to, workstation, line, EntryLimit, cancellationToken)
            : [];

        var entries = foundPresences
            .Select(presence => StationMapping.ToEntry(presence, openUntil))
            .Concat(foundVacancies.Select(vacancy => StationMapping.ToEntry(vacancy, openUntil)))
            .Concat(foundUnknowns.Select(unknown => StationMapping.ToEntry(unknown, openUntil)))
            .OrderByDescending(entry => entry.StartedAt)
            .ToList();

        var totals = StationMapping.DayTotals(entries);

        var pageSize = ClampPageSize(query.PageSize);
        var totalPages = entries.Count == 0 ? 0 : (int)Math.Ceiling(entries.Count / (double)pageSize);
        var page = ClampPage(query.Page, totalPages);

        IReadOnlyList<StationTimelineDayResponse> days = [.. entries
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .GroupBy(entry => DateOnly.FromDateTime(entry.StartedAt))
            .OrderByDescending(day => day.Key)
            .Select(day => StationMapping.ToDay(day, totals[day.Key]))];

        var truncated = foundPresences.Count >= EntryLimit
            || foundVacancies.Count >= EntryLimit
            || foundUnknowns.Count >= EntryLimit;

        return Result.Ok(new StationTimelineResponse(
            from,
            to,
            page,
            pageSize,
            entries.Count,
            totalPages,
            totals.Count,
            totals.Values.Sum(day => day.OccupiedSeconds),
            totals.Values.Sum(day => day.VacantSeconds),
            entries.Where(entry => entry.Registration is not null)
                .Select(entry => entry.Registration)
                .Distinct()
                .Count(),
            truncated,
            days,
            totals.Values.Sum(day => day.UnknownSeconds)));
    }

    internal static (DateTime From, DateTime To) ResolveWindow(StationTimelineQuery query, DateTime now)
    {
        var to = query.To ?? now;
        var from = query.From ?? to.Date;

        if (to <= from) to = from.AddDays(1);

        var ceiling = from.AddDays(MaxRangeDays);

        return (from, to > ceiling ? ceiling : to);
    }

    private static int ClampPageSize(int? requested) =>
        requested is null or < 1 ? DefaultPageSize : Math.Min(requested.Value, MaxPageSize);

    private static int ClampPage(int? requested, int totalPages)
    {
        var page = requested is null or < 1 ? 1 : requested.Value;

        return totalPages > 0 && page > totalPages ? totalPages : page;
    }

    private static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
