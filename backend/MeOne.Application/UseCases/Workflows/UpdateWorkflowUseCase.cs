using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class UpdateWorkflowUseCase(IWorkflowRepository workflows, ICurrentActor currentActor) : IUpdateWorkflowUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail(new NotFoundError(WorkflowErrors.NotFound));

        if (workflow.CheckEdit(actorId.Value) is { } refusal)
            return WorkflowsMapping.Fail(refusal);

        if (!workflow.Edit(
                actorId.Value,
                request.Title,
                request.Description ?? string.Empty,
                request.Priority,
                request.Category,
                DateTime.UtcNow))
            return Result.Ok();

        workflows.Update(workflow);

        return await workflows.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(WorkflowErrors.NothingPersisted);
    }
}
