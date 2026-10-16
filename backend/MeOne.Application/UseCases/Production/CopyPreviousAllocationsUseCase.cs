using FluentResults;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CopyPreviousAllocationsUseCase(
    IProductionRunRepository runs,
    IUserRepository users,
    ICurrentActor currentActor) : ICopyPreviousAllocationsUseCase
{
    public async Task<Result<CopyAllocationsResponse>> ExecuteAsync(ulong runId, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<CopyAllocationsResponse>();

        var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
        if (run.IsFailed)
            return run.ToResult<CopyAllocationsResponse>();

        if (!run.Value.IsActive)
            return Result.Fail<CopyAllocationsResponse>(ProductionErrors.RunClosed);

        var previous = await runs.GetLastClosedOfLayoutAsync(run.Value.LayoutId, cancellationToken);
        if (previous is null)
            return Result.Fail<CopyAllocationsResponse>(new NotFoundError(ProductionErrors.NoPreviousRun));

        var candidateIds = previous.Allocations.Select(allocation => allocation.UserId).Distinct().ToList();
        var active = (await users.GetActiveByIdsAsync(candidateIds, cancellationToken)).Select(user => user.Id).ToHashSet();
        var busy = await runs.FindActiveRunsOfUsersAsync(candidateIds, runId, cancellationToken);

        var (allocated, skipped) = run.Value.CopyAllocationsFrom(
            previous,
            userId => active.Contains(userId) && !busy.ContainsKey(userId),
            ProductionMapping.MaxMachinePostsPerPerson,
            actorId.Value,
            DateTime.UtcNow);

        if (allocated == 0)
            return Result.Ok(new CopyAllocationsResponse(0, skipped));

        runs.Update(run.Value);

        return await runs.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(new CopyAllocationsResponse(allocated, skipped))
            : Result.Fail<CopyAllocationsResponse>(ProductionErrors.NothingPersisted);
    }
}
