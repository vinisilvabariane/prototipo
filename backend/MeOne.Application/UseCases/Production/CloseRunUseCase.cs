using FluentResults;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CloseRunUseCase(IProductionRunRepository runs, ICurrentActor currentActor) : ICloseRunUseCase
{
    public async Task<Result> ExecuteAsync(ulong runId, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult();

        var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
        if (run.IsFailed)
            return run.ToResult();

        if (run.Value.CheckClose() is { } problem)
            return Result.Fail(problem);

        run.Value.Close(actorId.Value, DateTime.UtcNow);
        runs.Update(run.Value);

        return await runs.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
