using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Workflows;

public sealed class RejectWorkflowUseCase(IWorkflowRepository workflows, ICurrentActor currentActor) : IRejectWorkflowUseCase
{
    public async Task<Result<WorkflowActionResponse>> ExecuteAsync(
        ulong id,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<WorkflowActionResponse>();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail<WorkflowActionResponse>(new NotFoundError(WorkflowErrors.NotFound));

        if (workflow.CheckReject(actorId.Value) is { } refusal)
            return WorkflowsMapping.Fail<WorkflowActionResponse>(refusal);

        workflow.Reject(actorId.Value, request.Justification, DateTime.UtcNow);
        workflows.Update(workflow);

        return await workflows.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(new WorkflowActionResponse(workflow.Status, WorkflowOutcomeTexts.RejectionRecorded))
            : Result.Fail<WorkflowActionResponse>(WorkflowErrors.NothingPersisted);
    }
}
