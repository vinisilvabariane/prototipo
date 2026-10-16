using MeOne.Domain.Entities;

namespace MeOne.Domain.Rules;

public sealed record PostProductivity(ulong PostId, double OccupiedMinutes, int Produced, decimal? Expected, decimal? Attainment);

public sealed record PersonProductivity(ulong UserId, double Minutes, int Produced);

public sealed record HourProductivity(DateTime HourStart, int Produced);

public sealed record RunProductivityReport(
    IReadOnlyList<PostProductivity> Posts,
    IReadOnlyList<PersonProductivity> People,
    IReadOnlyList<HourProductivity> Hours,
    int TotalProduced);

public static class RunProductivity
{
    public static RunProductivityReport Compute(ProductionRun run, DateTime now)
    {
        if (run.StartedAt is not { } startedAt)
            return new([.. run.Snapshot.Posts.Select(post => new PostProductivity(post.Id, 0, 0, null, null))], [], [], 0);

        var windowEnd = run.EndedAt ?? now;

        var posts = run.Snapshot.Posts.Select(post =>
        {
            var minutes = run.Allocations
                .Where(allocation => allocation.PostId == post.Id)
                .Sum(allocation => Minutes(allocation.AllocatedAt, allocation.ReleasedAt ?? windowEnd, startedAt, windowEnd));

            var produced = run.Outputs.Where(output => output.PostId == post.Id).Sum(output => output.Quantity);

            decimal? expected = post.TargetPerHour is { } target && minutes > 0
                ? Math.Round(target * (decimal)minutes / 60m, 2)
                : null;

            decimal? attainment = expected is > 0 ? Math.Round(produced / expected.Value, 4) : null;

            return new PostProductivity(post.Id, Math.Round(minutes, 1), produced, expected, attainment);
        }).ToList();

        var people = run.Allocations
            .GroupBy(allocation => allocation.UserId)
            .Select(group => new PersonProductivity(
                group.Key,
                Math.Round(UnionMinutes(group.Select(allocation => (allocation.AllocatedAt, allocation.ReleasedAt ?? windowEnd)), startedAt, windowEnd), 1),
                run.Outputs.Where(output => output.UserId == group.Key).Sum(output => output.Quantity)))
            .OrderByDescending(person => person.Produced)
            .ThenBy(person => person.UserId)
            .ToList();

        var hours = run.Outputs
            .GroupBy(output => output.HourStart)
            .Select(group => new HourProductivity(group.Key, group.Sum(output => output.Quantity)))
            .OrderBy(hour => hour.HourStart)
            .ToList();

        return new(posts, people, hours, run.Outputs.Sum(output => output.Quantity));
    }

    private static double Minutes(DateTime from, DateTime to, DateTime windowFrom, DateTime windowTo)
    {
        var start = from > windowFrom ? from : windowFrom;
        var end = to < windowTo ? to : windowTo;

        return end > start ? (end - start).TotalMinutes : 0;
    }

    private static double UnionMinutes(IEnumerable<(DateTime From, DateTime To)> intervals, DateTime windowFrom, DateTime windowTo)
    {
        var clipped = intervals
            .Select(interval => (From: interval.From > windowFrom ? interval.From : windowFrom, To: interval.To < windowTo ? interval.To : windowTo))
            .Where(interval => interval.To > interval.From)
            .OrderBy(interval => interval.From)
            .ToList();

        double total = 0;
        DateTime? currentFrom = null;
        DateTime currentTo = default;

        foreach (var interval in clipped)
        {
            if (currentFrom is null)
            {
                currentFrom = interval.From;
                currentTo = interval.To;
            }
            else if (interval.From <= currentTo)
            {
                if (interval.To > currentTo)
                    currentTo = interval.To;
            }
            else
            {
                total += (currentTo - currentFrom.Value).TotalMinutes;
                currentFrom = interval.From;
                currentTo = interval.To;
            }
        }

        if (currentFrom is not null)
            total += (currentTo - currentFrom.Value).TotalMinutes;

        return total;
    }
}
