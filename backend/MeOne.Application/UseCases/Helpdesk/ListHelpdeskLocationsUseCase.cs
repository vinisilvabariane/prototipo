using FluentResults;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class ListHelpdeskLocationsUseCase(IProductionLineRepository lines, IProductionRunRepository runs) : IListHelpdeskLocationsUseCase
{
    private const int MaxRunning = 200;

    public async Task<Result<IReadOnlyList<HelpdeskLocationResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var active = await lines.ListAsync(false, cancellationToken);
        var running = await runs.SearchAsync(RunStatus.Running, null, 0, MaxRunning, cancellationToken);
        var byLine = running.GroupBy(run => run.LineId).ToDictionary(group => group.Key, group => group.OrderByDescending(run => run.StartedAt).First());

        var items = active
            .Select(line => new HelpdeskLocationResponse(
                line.Id,
                line.Name,
                byLine.TryGetValue(line.Id, out var run)
                    ? [.. run.Snapshot.Posts.OrderBy(post => post.X).ThenBy(post => post.Id).Select(post => new HelpdeskPostResponse(post.Id, post.Name))]
                    : []))
            .ToList();

        return Result.Ok<IReadOnlyList<HelpdeskLocationResponse>>(items);
    }
}
