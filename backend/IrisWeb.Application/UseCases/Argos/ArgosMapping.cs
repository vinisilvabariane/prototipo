using IrisWeb.Application.Communication.Argos.Responses;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Argos;

internal static class ArgosMapping
{
    public const string NoStation = "(sem estação)";

    public const string OkStatus = "ok";

    public const string NgStatus = "ng";

    public static string? Trimmed(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static ArgosCycleResponse ToCycle(ArgosCycle cycle) => new(
        cycle.Id,
        cycle.RecordedAt,
        cycle.IsOk ? OkStatus : NgStatus,
        (double)cycle.CycleSeconds,
        cycle.Station,
        cycle.SetupName,
        cycle.FailReason,
        cycle.FailedPoint,
        cycle.MinConfidence is { } confidence ? Math.Round((double)confidence, 3) : null);

    public static ArgosSummaryResponse ToSummary(
        DateTime from,
        DateTime to,
        ArgosBucket bucket,
        DateTime generatedAt,
        DateTime? lastCycleAt,
        IReadOnlyList<ArgosCycle> rows,
        IReadOnlyList<ArgosCycle> rowsOfAnyShift) =>
        new(
            from,
            to,
            bucket.ToString().ToLowerInvariant(),
            generatedAt,
            lastCycleAt,
            new ArgosTotalsResponse(Count(rows, true), Count(rows, false), Average(rows)),
            new ArgosByShiftResponse(
                CountOf(rowsOfAnyShift, ArgosShiftFilter.Adm),
                CountOf(rowsOfAnyShift, ArgosShiftFilter.T2)),
            Buckets(rows, bucket),
            FailReasons(rows),
            FailedPoints(rows),
            Stations(rows));

    private static ArgosCountResponse CountOf(IEnumerable<ArgosCycle> rows, ArgosShiftFilter shift)
    {
        var inShift = rows.Where(cycle => ArgosShiftRule.Matches(shift, cycle.RecordedAt)).ToList();

        return new ArgosCountResponse(Count(inShift, true), Count(inShift, false));
    }

    private static int Count(IEnumerable<ArgosCycle> rows, bool ok) => rows.Count(cycle => cycle.IsOk == ok);

    private static double? Average(IReadOnlyCollection<ArgosCycle> rows) =>
        rows.Count == 0 ? null : (double)Math.Round(rows.Average(cycle => cycle.CycleSeconds), 2);

    private static IReadOnlyList<ArgosBucketResponse> Buckets(IEnumerable<ArgosCycle> rows, ArgosBucket bucket) =>
        [.. rows
            .GroupBy(cycle => BucketStart(cycle.RecordedAt, bucket))
            .OrderBy(group => group.Key)
            .Select(group => new ArgosBucketResponse(
                group.Key,
                Count(group, true),
                Count(group, false),
                Average([.. group])))];

    private static IReadOnlyList<ArgosFailReasonResponse> FailReasons(IEnumerable<ArgosCycle> rows) =>
        [.. rows
            .Where(cycle => !cycle.IsOk)
            .GroupBy(cycle => cycle.FailReason)
            .Select(group => new ArgosFailReasonResponse(group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Reason, StringComparer.Ordinal)];

    private static IReadOnlyList<ArgosFailedPointResponse> FailedPoints(IEnumerable<ArgosCycle> rows) =>
        [.. rows
            .Where(cycle => !cycle.IsOk && cycle.FailedPoint is not null)
            .GroupBy(cycle => cycle.FailedPoint!)
            .Select(group => new ArgosFailedPointResponse(group.Key, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Point, StringComparer.Ordinal)];

    private static IReadOnlyList<ArgosStationResponse> Stations(IEnumerable<ArgosCycle> rows) =>
        [.. rows
            .GroupBy(cycle => cycle.Station ?? NoStation)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ArgosStationResponse(
                group.Key,
                Count(group, true),
                Count(group, false),
                Average([.. group]),
                group.Max(cycle => cycle.RecordedAt)))];

    private static DateTime BucketStart(DateTime at, ArgosBucket bucket) => bucket switch
    {
        ArgosBucket.Minute => new DateTime(at.Year, at.Month, at.Day, at.Hour, at.Minute, 0, at.Kind),
        ArgosBucket.Day => at.Date,
        _ => new DateTime(at.Year, at.Month, at.Day, at.Hour, 0, 0, at.Kind),
    };
}
