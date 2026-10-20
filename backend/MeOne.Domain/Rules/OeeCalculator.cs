using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Domain.Rules;

public sealed record PillarValues(double Availability, double Performance, double Quality, double Oee);

public sealed record HourOee(DateTime HourStart, double Seconds, PillarValues Pillars, double Produced, double Theoretical);

public sealed record PostOee(
    ulong PostId,
    string Name,
    PostKind Kind,
    int X,
    decimal? TargetPerHour,
    bool Measured,
    PillarValues Pillars,
    IReadOnlyDictionary<string, double> Variables,
    IReadOnlyList<HourOee> Hours);

public sealed record LineOee(
    PillarValues Pillars,
    IReadOnlyDictionary<string, double> Variables,
    IReadOnlyList<HourOee> Hours,
    ulong? LastPostId);

public sealed record RunOee(
    bool Started,
    DateTime? From,
    DateTime? To,
    double ElapsedSeconds,
    IReadOnlyList<PostOee> Posts,
    LineOee Line,
    bool LossSourceAvailable);

public sealed class OeeCalculator
{
    private const double HourSeconds = 3600;

    private static readonly PillarValues Zero = new(0, 0, 0, 0);

    private readonly OeeExpression _availability;
    private readonly OeeExpression _performance;
    private readonly OeeExpression _quality;
    private readonly OeeExpression _machineOee;
    private readonly OeeExpression _lineAvailability;
    private readonly OeeExpression _linePerformance;
    private readonly OeeExpression _lineQuality;
    private readonly OeeExpression _lineOee;

    public OeeCalculator(OeeFormulas formulas)
    {
        _availability = Compile(formulas.Availability, OeeFormulas.Default.Availability);
        _performance = Compile(formulas.Performance, OeeFormulas.Default.Performance);
        _quality = Compile(formulas.Quality, OeeFormulas.Default.Quality);
        _machineOee = Compile(formulas.MachineOee, OeeFormulas.Default.MachineOee);
        _lineAvailability = Compile(formulas.LineAvailability, OeeFormulas.Default.LineAvailability);
        _linePerformance = Compile(formulas.LinePerformance, OeeFormulas.Default.LinePerformance);
        _lineQuality = Compile(formulas.LineQuality, OeeFormulas.Default.LineQuality);
        _lineOee = Compile(formulas.LineOee, OeeFormulas.Default.LineOee);
    }

    public static OeeCalculator Default { get; } = new(OeeFormulas.Default);

    public RunOee Calculate(ProductionRun run, DateTime now, OeeLosses losses)
    {
        if (run.StartedAt is not { } started)
        {
            var empty = run.Snapshot.Posts
                .Select(post => new PostOee(post.Id, post.Name, post.Kind, post.X, post.TargetPerHour, false, Zero, new Dictionary<string, double>(), []))
                .ToList();

            return new(false, null, null, 0, empty, new(Zero, new Dictionary<string, double>(), [], null), losses.Available);
        }

        var to = run.EndedAt ?? now;
        var slots = SlotsBetween(started, to);

        var lastPostId = run.Snapshot.Posts
            .Where(post => post.TargetPerHour is > 0)
            .OrderBy(post => post.X)
            .ThenBy(post => post.Id)
            .Select(post => (ulong?)post.Id)
            .LastOrDefault();

        var posts = run.Snapshot.Posts
            .Select(post => CalculatePost(run, post, started, to, slots, losses, lastPostId))
            .ToList();

        var line = CalculateLine(posts, started, to, slots);

        return new(true, started, to, Math.Max((to - started).TotalSeconds, 0), posts, line, losses.Available);
    }

    public (double Value, IReadOnlyDictionary<string, double> Variables) EvaluatePostPillar(string pillar, IReadOnlyDictionary<string, double> variables)
    {
        var expression = pillar switch
        {
            "availability" => _availability,
            "performance" => _performance,
            _ => _quality,
        };

        return (Math.Max(0, expression.Evaluate(variables)), variables);
    }

    private PostOee CalculatePost(
        ProductionRun run,
        RunPostSnapshot post,
        DateTime from,
        DateTime to,
        IReadOnlyList<DateTime> slots,
        OeeLosses losses,
        ulong? lastPostId)
    {
        var measured = post.TargetPerHour is > 0;
        var total = PostVariables(run, post, from, to, losses, lastPostId, wholeRun: true);
        var pillars = measured ? Pillars(total) : Zero;

        var hours = measured
            ? slots.Select(slot =>
            {
                var windowFrom = Max(slot, from);
                var windowTo = Min(slot.AddHours(1), to);
                var vars = PostVariables(run, post, windowFrom, windowTo, losses, lastPostId, wholeRun: false, slot);
                return new HourOee(slot, Math.Max((windowTo - windowFrom).TotalSeconds, 0), Pillars(vars), vars["real_production"], vars["theoretical_production"]);
            }).ToList()
            : [];

        return new(post.Id, post.Name, post.Kind, post.X, post.TargetPerHour, measured, pillars, total, hours);
    }

    private PillarValues Pillars(Dictionary<string, double> variables)
    {
        var availability = Math.Max(0, _availability.Evaluate(variables));
        var performance = Math.Max(0, _performance.Evaluate(variables));
        var quality = Math.Max(0, _quality.Evaluate(variables));

        var pillarVariables = new Dictionary<string, double>(variables)
        {
            ["availability"] = availability,
            ["performance"] = performance,
            ["quality"] = quality,
        };

        return new(availability, performance, quality, Math.Max(0, _machineOee.Evaluate(pillarVariables)));
    }

    private static Dictionary<string, double> PostVariables(
        ProductionRun run,
        RunPostSnapshot post,
        DateTime from,
        DateTime to,
        OeeLosses losses,
        ulong? lastPostId,
        bool wholeRun,
        DateTime? slot = null)
    {
        var elapsed = Math.Max((to - from).TotalSeconds, 0);
        var target = (double)(post.TargetPerHour ?? 0);

        var occupied = run.Allocations
            .Where(allocation => allocation.PostId == post.Id)
            .Sum(allocation => Overlap(allocation.AllocatedAt, allocation.ReleasedAt ?? to, from, to));

        var produced = run.Outputs
            .Where(output => output.PostId == post.Id && (wholeRun || output.HourStart == slot))
            .Sum(output => output.Quantity);

        var relevant = losses.Downtimes.Where(downtime => downtime.PostId is null || downtime.PostId == post.Id).ToList();

        double Seconds(Func<OeeDowntimeEvent, bool> filter) =>
            relevant.Where(filter).Sum(downtime => Overlap(downtime.From, downtime.To, from, to));

        var ticket = Seconds(downtime => downtime.Source == OeeDowntimeSource.Ticket);
        var defects = DefectQuantity(losses, post.Id, lastPostId, from, to, repair: false, inclusiveEnd: wholeRun);
        var repairs = DefectQuantity(losses, post.Id, lastPostId, from, to, repair: true, inclusiveEnd: wholeRun);

        var theoretical = target * elapsed / HourSeconds;
        var expected = target * occupied / HourSeconds;

        return new Dictionary<string, double>
        {
            ["elapsed_seconds"] = elapsed,
            ["oee_time_rate_seconds"] = elapsed,
            ["planned_stop_seconds"] = 0,
            ["occupied_seconds"] = occupied,
            ["downtime_availability"] = Seconds(downtime => downtime.AffectsAvailability),
            ["downtime_performance"] = Seconds(downtime => downtime.AffectsPerformance),
            ["total_downtime"] = Seconds(_ => true),
            ["real_downtime"] = Seconds(downtime => downtime.Source == OeeDowntimeSource.Real),
            ["ticket_downtime_proportional"] = ticket,
            ["ticket_downtime_pieces"] = target / HourSeconds * ticket,
            ["real_production"] = produced,
            ["theoretical_production"] = theoretical,
            ["expected_production"] = expected,
            ["expected_production_min"] = expected * 0.9,
            ["max_rate_hour"] = theoretical,
            ["defects"] = defects,
            ["repair_defects"] = repairs,
        };
    }

    private LineOee CalculateLine(List<PostOee> posts, DateTime from, DateTime to, IReadOnlyList<DateTime> slots)
    {
        var measured = posts.Where(post => post.Measured).OrderBy(post => post.X).ThenBy(post => post.PostId).ToList();
        var last = measured.LastOrDefault();

        var variables = LineVariables(measured, last, [.. measured.Select(post => post.Pillars)], Math.Max((to - from).TotalSeconds, 0));
        var pillars = LinePillars(variables);

        var hours = slots.Select(slot =>
        {
            var perPost = measured
                .Select(post => post.Hours.FirstOrDefault(hour => hour.HourStart == slot))
                .Where(hour => hour is not null)
                .Select(hour => hour!)
                .ToList();

            var lastHour = last?.Hours.FirstOrDefault(hour => hour.HourStart == slot);
            var seconds = perPost.Count == 0 ? 0 : perPost.Max(hour => hour.Seconds);
            var hourVariables = LineVariables(measured, last, [.. perPost.Select(hour => hour.Pillars)], seconds, perPost, lastHour);

            return new HourOee(slot, seconds, LinePillars(hourVariables), perPost.Sum(hour => hour.Produced), lastHour?.Theoretical ?? 0);
        }).ToList();

        return new(pillars, variables, hours, last?.PostId);
    }

    private Dictionary<string, double> LineVariables(
        List<PostOee> measured,
        PostOee? last,
        List<PillarValues> values,
        double elapsed,
        List<HourOee>? hourly = null,
        HourOee? lastHour = null)
    {
        double Sum(string key) => measured.Sum(post => post.Variables.GetValueOrDefault(key));

        var variables = new Dictionary<string, double>
        {
            ["line_plan"] = lastHour?.Theoretical ?? last?.Variables.GetValueOrDefault("theoretical_production") ?? 0,
            ["line_produced"] = hourly?.Sum(hour => hour.Produced) ?? Sum("real_production"),
            ["line_produced_station"] = lastHour?.Produced ?? last?.Variables.GetValueOrDefault("real_production") ?? 0,
            ["line_elapsed"] = elapsed,
            ["line_downtime_availability"] = Sum("downtime_availability"),
            ["line_downtime_performance"] = Sum("downtime_performance"),
            ["line_total_downtime"] = Sum("total_downtime"),
            ["line_defects"] = Sum("defects"),
        };

        void Aggregate(string pillar, Func<PillarValues, double> pick, PillarValues? lastValues)
        {
            var ratios = values.Select(pick).ToList();

            variables[OeeCatalog.AggregateKey(pillar, "average")] = ratios.Count == 0 ? 0 : ratios.Average();
            variables[OeeCatalog.AggregateKey(pillar, "sum")] = ratios.Sum();
            variables[OeeCatalog.AggregateKey(pillar, "product")] = ratios.Count == 0 ? 0 : ratios.Aggregate(1.0, (accumulator, ratio) => accumulator * ratio);
            variables[OeeCatalog.AggregateKey(pillar, "last_post")] = lastValues is not null ? pick(lastValues) : (ratios.Count == 0 ? 0 : ratios.Average());
        }

        var lastValues = lastHour?.Pillars ?? last?.Pillars;

        Aggregate("availability", value => value.Availability, lastValues);
        Aggregate("performance", value => value.Performance, lastValues);
        Aggregate("quality", value => value.Quality, lastValues);
        Aggregate("machine_oee", value => value.Oee, lastValues);

        return variables;
    }

    private PillarValues LinePillars(Dictionary<string, double> variables)
    {
        var availability = Math.Max(0, _lineAvailability.Evaluate(variables));
        var performance = Math.Max(0, _linePerformance.Evaluate(variables));
        var quality = Math.Max(0, _lineQuality.Evaluate(variables));

        var oeeVariables = new Dictionary<string, double>(variables)
        {
            ["availability"] = availability,
            ["performance"] = performance,
            ["quality"] = quality,
            ["machine_oee"] = variables.GetValueOrDefault(OeeCatalog.AggregateKey("machine_oee", "average")),
        };

        return new(availability, performance, quality, Math.Max(0, _lineOee.Evaluate(oeeVariables)));
    }

    private static double DefectQuantity(OeeLosses losses, ulong postId, ulong? lastPostId, DateTime from, DateTime to, bool repair, bool inclusiveEnd) =>
        losses.Defects
            .Where(defect => defect.IsRepair == repair && (defect.PostId == postId || (defect.PostId is null && postId == lastPostId)))
            .Where(defect => defect.At >= from && (inclusiveEnd ? defect.At <= to : defect.At < to))
            .Sum(defect => defect.Quantity);

    private static List<DateTime> SlotsBetween(DateTime from, DateTime to)
    {
        var slots = new List<DateTime>();

        for (var slot = ProductionRun.TruncateToHour(from); slot < to || slots.Count == 0; slot = slot.AddHours(1))
        {
            slots.Add(slot);

            if (slots.Count >= 96)
                break;
        }

        return slots;
    }

    private static double Overlap(DateTime from, DateTime to, DateTime windowFrom, DateTime windowTo)
    {
        var start = Max(from, windowFrom);
        var end = Min(to, windowTo);

        return end > start ? (end - start).TotalSeconds : 0;
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static OeeExpression Compile(string source, string fallback)
    {
        var (expression, _) = OeeFormula.Parse(source);

        return expression ?? OeeFormula.Parse(fallback).Expression!;
    }
}
