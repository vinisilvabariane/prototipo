using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeArgosCycleRepository : InMemoryRepository<ArgosCycle, long>, IArgosCycleRepository
{
    private IEnumerable<ArgosCycle> Match(ArgosCycleFilter filter) => Items
        .Where(cycle => cycle.RecordedAt >= filter.From && cycle.RecordedAt < filter.To)
        .Where(cycle => filter.Result switch
        {
            ArgosResultFilter.Ok => cycle.IsOk,
            ArgosResultFilter.Ng => !cycle.IsOk,
            _ => true,
        })
        .Where(cycle => filter.Station is null || cycle.Station == filter.Station)
        .Where(cycle => filter.Setup is null || cycle.SetupName == filter.Setup)
        .Where(cycle => ArgosShiftRule.Matches(filter.Shift, cycle.RecordedAt));

    public Task<IReadOnlyList<ArgosCycle>> ListAsync(ArgosCycleFilter filter, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ArgosCycle>>([.. Match(filter).OrderBy(cycle => cycle.RecordedAt)]);

    public Task<(IReadOnlyList<ArgosCycle> Entries, int Total, int Page)> PageAsync(
        ArgosCycleFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var all = Match(filter).OrderByDescending(cycle => cycle.RecordedAt).ThenByDescending(cycle => cycle.Id).ToList();
        var totalPages = all.Count == 0 ? 1 : (int)Math.Ceiling(all.Count / (double)pageSize);
        var effective = Math.Clamp(page, 1, totalPages);

        IReadOnlyList<ArgosCycle> entries = [.. all.Skip((effective - 1) * pageSize).Take(pageSize)];

        return Task.FromResult((entries, all.Count, effective));
    }

    public Task<DateTime?> LastRecordedAtAsync(string? station, string? setup, CancellationToken cancellationToken) =>
        Task.FromResult(Items
            .Where(cycle => station is null || cycle.Station == station)
            .Where(cycle => setup is null || cycle.SetupName == setup)
            .Select(cycle => (DateTime?)cycle.RecordedAt)
            .Max());

    public Task<IReadOnlyList<string>> ListStationsAsync(DateTime since, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>([.. Items
            .Where(cycle => cycle.RecordedAt >= since && cycle.Station is not null)
            .Select(cycle => cycle.Station!)
            .Distinct()
            .OrderBy(station => station)]);

    public Task<IReadOnlyList<string>> ListSetupsAsync(DateTime since, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>([.. Items
            .Where(cycle => cycle.RecordedAt >= since && cycle.SetupName is not null)
            .Select(cycle => cycle.SetupName!)
            .Distinct()
            .OrderBy(setup => setup)]);
}
