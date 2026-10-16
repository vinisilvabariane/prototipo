using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class AllocatePostUseCase(
    IProductionRunRepository runs,
    IProductionLineRepository lines,
    IUserRepository users,
    ICurrentActor currentActor) : IAllocatePostUseCase
{
    public async Task<Result> ExecuteAsync(ulong runId, ulong postId, AllocatePostRequest request, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult();

        var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
        if (run.IsFailed)
            return run.ToResult();

        var person = (await users.GetActiveByIdsAsync([request.UserId], cancellationToken)).FirstOrDefault();
        if (person is null)
            return Result.Fail(new NotFoundError(ProductionErrors.UserNotFound));

        if (run.Value.CheckAllocate(postId, request.UserId, ProductionMapping.MaxMachinePostsPerPerson) is { } problem)
        {
            return problem == ProductionErrors.PostNotInRun
                ? Result.Fail(new NotFoundError(problem))
                : Result.Fail(problem);
        }

        var busy = await runs.FindActiveRunsOfUsersAsync([request.UserId], runId, cancellationToken);

        if (busy.TryGetValue(request.UserId, out var busyLineId))
        {
            var line = (await lines.GetByIdsAsync([busyLineId], cancellationToken)).FirstOrDefault();
            return Result.Fail(ProductionErrors.PersonBusy(person.Name, line?.Name ?? string.Empty));
        }

        if (!run.Value.Allocate(postId, request.UserId, actorId.Value, DateTime.UtcNow))
            return Result.Ok();

        runs.Update(run.Value);

        return await runs.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
