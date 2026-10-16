using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class ReturnRejectionToAssigneeUseCase(IWorkflowRepository workflows, ICurrentActor currentActor) : IReturnRejectionToAssigneeUseCase
{
    public async Task<Result> ExecuteAsync(
        ulong id,
        ulong stepId,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail(new NotFoundError(WorkflowErrors.NotFound));

        if (workflow.CheckReturnToAssignee(actorId.Value, stepId) is { } refusal)
            return WorkflowsMapping.Fail(refusal);

        workflow.ReturnToAssignee(actorId.Value, stepId, request.Justification, DateTime.UtcNow);
        workflows.Update(workflow);

        return await workflows.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(WorkflowErrors.NothingPersisted);
    }
}
