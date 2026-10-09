using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Stations;

/// <summary>
/// Linha do tempo da linha num turno: posto a posto, quem esteve (e se era treinado), quando
/// ficou vazio e quando havia rosto que ninguem reconheceu. Le as tres tabelas do Iris; o
/// nivel vem do retrato gravado quando a presenca abriu.
/// </summary>
public sealed class GetLineTimelineUseCase(
    IStationPresenceRepository presences,
    IStationVacancyRepository vacancies,
    IStationUnknownRepository unknowns) : IGetLineTimelineUseCase
{
    public const int MaxRows = 5000;

    public async Task<Result<LineTimelineResponse>> ExecuteAsync(LineTimelineQuery query, CancellationToken cancellationToken)
    {
        var line = StationNamingPolicy.NormalizeField(query.Line);
        var (from, to) = Window(query.Date!.Value, query.From, query.To);
        var now = DateTime.Now;
        var end = to < now ? to : now;

        if (end <= from)
            return Result.Ok(Empty(line, from, to));

        var presenceRows = await presences.ListOverlappingAsync(from, end, null, line, null, MaxRows, cancellationToken);
        var vacancyRows = await vacancies.ListOverlappingAsync(from, end, null, line, MaxRows, cancellationToken);
        var unknownRows = await unknowns.ListOverlappingAsync(from, end, null, line, MaxRows, cancellationToken);
        var truncated = presenceRows.Count >= MaxRows || vacancyRows.Count >= MaxRows || unknownRows.Count >= MaxRows;

        var segments = new List<(string Workstation, LineTimelineSegmentResponse Segment)>();

        foreach (var row in presenceRows)
        {
            if (Clip(row.EnteredAt, row.LeftAt, from, end) is not { } span) continue;

            var training = TrainingLevelPolicy.Classify(row.TrainingLevel);
            bool? matches = row.AllocatedRegistration is null
                ? null
                : Normalize(row.AllocatedRegistration) == Normalize(row.Registration);

            segments.Add((row.Workstation, new LineTimelineSegmentResponse(
                "presence", span.Start, span.End, span.Open,
                row.Registration, row.CollaboratorName, row.TrainingLevel, Label(training),
                row.AllocatedRegistration, matches)));
        }

        foreach (var row in vacancyRows)
        {
            if (Clip(row.StartedAt, row.EndedAt, from, end) is not { } span) continue;
            segments.Add((row.Workstation, new LineTimelineSegmentResponse("vacancy", span.Start, span.End, span.Open)));
        }

        foreach (var row in unknownRows)
        {
            if (Clip(row.StartedAt, row.EndedAt, from, end) is not { } span) continue;
            segments.Add((row.Workstation, new LineTimelineSegmentResponse(
                "unknown", span.Start, span.End, span.Open,
                CandidateRegistration: row.CandidateRegistration, CandidateDistance: row.CandidateDistance)));
        }

        var window = Seconds(from, end);

        var stations = segments
            .GroupBy(item => item.Workstation)
            .Select(group =>
            {
                var parsed = StationNamingPolicy.ParseWorkstation(group.Key);
                var ordered = group.Select(item => item.Segment).OrderBy(segment => segment.StartedAt).ToList();
                return new LineTimelineStationResponse(
                    group.Key,
                    parsed?.StationId,
                    parsed?.Name,
                    Totals(ordered, window, 1),
                    ordered);
            })
            .OrderBy(station => station.StationId ?? int.MaxValue)
            .ThenBy(station => station.Workstation, StringComparer.Ordinal)
            .ToList();

        var all = stations.SelectMany(station => station.Segments).ToList();

        var people = stations
            .SelectMany(station => station.Segments
                .Where(segment => segment.Kind == "presence")
                .Select(segment => (station.Workstation, Segment: segment)))
            .GroupBy(item => Normalize(item.Segment.Registration!))
            .Select(group =>
            {
                var latest = group.OrderByDescending(item => item.Segment.StartedAt).ToList();
                var level = latest.Select(item => item.Segment.TrainingLevel).FirstOrDefault(value => value is not null);
                return new LineTimelinePersonResponse(
                    latest[0].Segment.Registration!,
                    latest.Select(item => item.Segment.Name).FirstOrDefault(value => value is not null),
                    level,
                    Label(TrainingLevelPolicy.Classify(level)),
                    group.Sum(item => Seconds(item.Segment.StartedAt, item.Segment.EndedAt)),
                    [.. group.Select(item => item.Workstation).Distinct().Order(StringComparer.Ordinal)]);
            })
            .OrderByDescending(person => person.Seconds)
            .ToList();

        return Result.Ok(new LineTimelineResponse(
            line,
            from,
            to,
            all.Count == 0 ? null : all.Min(segment => segment.StartedAt),
            all.Count == 0 ? null : all.Max(segment => segment.EndedAt),
            truncated,
            Totals(all, window * stations.Count, stations.Count),
            stations,
            people));
    }

    public static (DateTime From, DateTime To) Window(DateOnly date, TimeOnly? from, TimeOnly? to)
    {
        if (from is null || to is null)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            return (start, start.AddDays(1));
        }

        var begin = date.ToDateTime(from.Value);
        var finish = date.ToDateTime(to.Value);
        if (finish <= begin) finish = finish.AddDays(1);
        return (begin, finish);
    }

    private static LineTimelineResponse Empty(string line, DateTime from, DateTime to) =>
        new(line, from, to, null, null, false, Totals([], 0, 0), [], []);

    private static LineTimelineTotalsResponse Totals(IReadOnlyCollection<LineTimelineSegmentResponse> segments, int window, int stationCount)
    {
        int Sum(Func<LineTimelineSegmentResponse, bool> filter) =>
            segments.Where(filter).Sum(segment => Seconds(segment.StartedAt, segment.EndedAt));

        return new LineTimelineTotalsResponse(
            window,
            stationCount,
            Sum(segment => segment.Kind == "presence"),
            Sum(segment => segment.Kind == "presence" && segment.Training == "trained"),
            Sum(segment => segment.Kind == "presence" && segment.Training == "untrained"),
            Sum(segment => segment.Kind == "presence" && segment.Training == "unknown"),
            Sum(segment => segment.Kind == "vacancy"),
            Sum(segment => segment.Kind == "unknown"),
            Sum(segment => segment.Kind == "presence" && segment.MatchesAllocation == false));
    }

    private static (DateTime Start, DateTime End, bool Open)? Clip(DateTime started, DateTime? ended, DateTime from, DateTime end)
    {
        var start = started < from ? from : started;
        var stop = ended is { } closed && closed < end ? closed : end;
        return stop > start ? (start, stop, ended is null) : null;
    }

    private static int Seconds(DateTime start, DateTime end) => (int)Math.Round((end - start).TotalSeconds);

    private static string Normalize(string registration)
    {
        var trimmed = registration.Trim().TrimStart('0');
        return trimmed.Length == 0 ? "0" : trimmed;
    }

    private static string Label(TrainingStatus status) => status switch
    {
        TrainingStatus.Trained => "trained",
        TrainingStatus.Untrained => "untrained",
        _ => "unknown",
    };
}
