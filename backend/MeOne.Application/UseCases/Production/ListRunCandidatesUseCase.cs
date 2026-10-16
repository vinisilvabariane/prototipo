using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Domain.Enums;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class ListRunCandidatesUseCase(
    IProductionRunRepository runs,
    IProductionLineRepository lines,
    IUserRepository users) : IListRunCandidatesUseCase
{
    private const int MaxCandidates = 100;

    public async Task<Result<IReadOnlyList<RunCandidateResponse>>> ExecuteAsync(
        ulong runId,
        CandidatesQuery query,
        CancellationToken cancellationToken)
    {
        var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
        if (run.IsFailed)
            return run.ToResult<IReadOnlyList<RunCandidateResponse>>();

        var term = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var matching = (await users.ListActiveAsync(cancellationToken))
            .Where(user => term is null
                || user.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                || user.Registration.ToString().Contains(term, StringComparison.Ordinal))
            .OrderBy(user => user.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(MaxCandidates)
            .ToList();

        var ids = matching.Select(user => user.Id).ToList();
        var busy = await runs.FindActiveRunsOfUsersAsync(ids, runId, cancellationToken);
        var lineNames = (await lines.GetByIdsAsync([.. busy.Values.Distinct()], cancellationToken)).ToDictionary(line => line.Id, line => line.Name);

        var open = run.Value.OpenAllocations;

        var items = matching.Select(user =>
        {
            var mine = open.Where(allocation => allocation.UserId == user.Id).ToList();

            return new RunCandidateResponse(
                user.Id,
                user.Name,
                user.Registration,
                user.Department?.Name ?? string.Empty,
                mine.Count(allocation => run.Value.Snapshot.FindPost(allocation.PostId)?.Kind == PostKind.Machine),
                mine.Any(allocation => run.Value.Snapshot.FindPost(allocation.PostId)?.Kind != PostKind.Machine),
                busy.TryGetValue(user.Id, out var lineId) ? lineNames.GetValueOrDefault(lineId, string.Empty) : null);
        }).ToList();

        return Result.Ok<IReadOnlyList<RunCandidateResponse>>(items);
    }
}
