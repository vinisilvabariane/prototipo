using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class RecordRunOutputUseCase(IProductionRunRepository runs, ICurrentActor currentActor) : IRecordRunOutputUseCase
{
    public async Task<Result> ExecuteAsync(ulong runId, RecordOutputRequest request, CancellationToken cancellationToken)
    {
        var actorId = await ProductionLookups.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult();

        var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
        if (run.IsFailed)
            return run.ToResult();

        var now = DateTime.UtcNow;
        var hour = request.HourStart is { } requested ? DateTime.SpecifyKind(requested.ToUniversalTime(), DateTimeKind.Utc) : now;

        if (run.Value.CheckRecordOutput(request.PostId, request.Quantity, hour, now) is { } problem)
        {
            return problem == ProductionErrors.PostNotInRun
                ? Result.Fail(new NotFoundError(problem))
                : Result.Fail(problem);
        }

        run.Value.RecordOutput(request.PostId, request.Quantity, hour, actorId.Value, now);
        runs.Update(run.Value);

        return await runs.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
