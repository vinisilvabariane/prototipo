using MeOne.Application.Communication.Production.Responses;
using MeOne.Domain.Entities;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Production;

internal static class ProductionMapping
{
    public const int MaxMachinePostsPerPerson = 5;

    public static ProductionLineResponse ToResponse(ProductionLine line) => new(line.Id, line.Name, line.Active, line.CreatedAt);

    public static ProductionShiftResponse ToResponse(ProductionShift shift) => new(shift.Id, shift.Name, shift.StartTime, shift.EndTime, shift.Active);

    public static ProductionProductResponse ToResponse(ProductionProduct product) => new(product.Id, product.Code, product.Name, product.Active);

    public static LayoutPostResponse ToResponse(LayoutPost post) => new(
        post.Id, post.Name, post.Kind, post.X, post.Y, post.Width, post.Height, post.Rotation, post.IsCritical, post.TargetPerHour, post.Operation);

    public static LayoutDecorationResponse ToResponse(LayoutDecoration decoration) => new(
        decoration.Id, decoration.Kind, decoration.X, decoration.Y, decoration.Width, decoration.Height, decoration.Rotation, decoration.Label, decoration.Locked);

    public static LayoutSummaryResponse ToSummary(
        LineLayout layout,
        ProductionLine line,
        ProductionProduct product,
        ProductionShift shift) => new(
        layout.Id,
        line.Id,
        line.Name,
        product.Id,
        product.Code,
        product.Name,
        shift.Id,
        shift.Name,
        layout.Posts.Count,
        layout.UpdatedAt);

    public static RunSummaryResponse ToSummary(
        ProductionRun run,
        ProductionLine line,
        ProductionProduct product,
        ProductionShift shift) => new(
        run.Id,
        run.Status,
        line.Id,
        line.Name,
        product.Code,
        product.Name,
        shift.Name,
        run.CreatedAt,
        run.StartedAt,
        run.EndedAt,
        run.OpenAllocations.Count,
        run.Snapshot.Posts.Count,
        run.Outputs.Sum(output => output.Quantity));

    public static RunDetailResponse ToDetail(
        ProductionRun run,
        ProductionLine line,
        ProductionProduct product,
        ProductionShift shift,
        IReadOnlyDictionary<ulong, User> users,
        DateTime now,
        bool canSupervise,
        bool canRecord)
    {
        var report = RunProductivity.Compute(run, now);
        var open = run.OpenAllocations.ToDictionary(allocation => allocation.PostId);
        var metrics = report.Posts.ToDictionary(item => item.PostId);

        string NameOf(ulong id) => users.TryGetValue(id, out var user) ? user.Name : string.Empty;
        uint RegistrationOf(ulong id) => users.TryGetValue(id, out var user) ? user.Registration : 0;

        var posts = run.Snapshot.Posts.Select(post =>
        {
            var metric = metrics[post.Id];

            RunAllocationResponse? allocation = open.TryGetValue(post.Id, out var current)
                ? new(current.UserId, NameOf(current.UserId), RegistrationOf(current.UserId), current.AllocatedAt)
                : null;

            return new RunPostResponse(
                post.Id, post.Name, post.Kind, post.X, post.Y, post.Width, post.Height, post.Rotation, post.IsCritical, post.TargetPerHour, post.Operation,
                allocation, metric.OccupiedMinutes, metric.Produced, metric.Expected, metric.Attainment);
        }).ToList();

        var people = report.People
            .Select(person => new RunPersonResponse(
                person.UserId,
                NameOf(person.UserId),
                RegistrationOf(person.UserId),
                person.Minutes,
                person.Produced,
                run.OpenAllocations.Count(allocation => allocation.UserId == person.UserId)))
            .ToList();

        return new RunDetailResponse(
            run.Id,
            run.Status,
            run.LayoutId,
            line.Id,
            line.Name,
            product.Code,
            product.Name,
            shift.Name,
            run.CreatedAt,
            NameOf(run.CreatedBy),
            run.StartedAt,
            run.StartedBy is { } startedBy ? NameOf(startedBy) : null,
            run.EndedAt,
            run.EndedBy is { } endedBy ? NameOf(endedBy) : null,
            posts,
            [.. run.Snapshot.Decorations.Select(decoration => new RunDecorationResponse(
                decoration.Kind, decoration.X, decoration.Y, decoration.Width, decoration.Height, decoration.Rotation, decoration.Label))],
            people,
            [.. report.Hours.Select(hour => new RunHourResponse(hour.HourStart, hour.Produced))],
            report.TotalProduced,
            canSupervise,
            canRecord);
    }

    public static IEnumerable<ulong> UserIdsOf(ProductionRun run) => run.Allocations.Select(allocation => allocation.UserId)
        .Concat(run.Outputs.Where(output => output.UserId is not null).Select(output => output.UserId!.Value))
        .Append(run.CreatedBy)
        .Concat(run.StartedBy is { } started ? [started] : [])
        .Concat(run.EndedBy is { } ended ? [ended] : []);
}
