using FluentResults;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class ReleaseAllPostsUseCase(IProductionRunRepository runs) : IReleaseAllPostsUseCase
{
    public async Task<Result> ExecuteAsync(ulong runId, CancellationToken cancellationToken)
    {
        var run = await ProductionLookups.OpenRunAsync(runs, runId, cancellationToken);
        if (run.IsFailed)
            return run.ToResult();

        if (!run.Value.IsActive)
            return Result.Fail(ProductionErrors.RunClosed);

        if (run.Value.ReleaseAll(DateTime.UtcNow) == 0)
            return Result.Ok();

        runs.Update(run.Value);

        return await runs.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
