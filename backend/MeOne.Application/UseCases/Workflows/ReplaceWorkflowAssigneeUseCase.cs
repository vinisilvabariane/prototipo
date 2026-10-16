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

public sealed class ReplaceWorkflowAssigneeUseCase(
    IWorkflowRepository workflows,
    IUserRepository users,
    ICurrentActor currentActor) : IReplaceWorkflowAssigneeUseCase
{
    public async Task<Result<WorkflowActionResponse>> ExecuteAsync(
        ulong id,
        ulong userId,
        ReplaceWorkflowAssigneeRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<WorkflowActionResponse>();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail<WorkflowActionResponse>(new NotFoundError(WorkflowErrors.NotFound));

        if (workflow.CheckReplaceAssignee(actorId.Value, userId, request.NewUserId) is { } refusal)
            return WorkflowsMapping.Fail<WorkflowActionResponse>(refusal);

        var activeNew = await users.GetActiveByIdsAsync([request.NewUserId], cancellationToken);
        if (activeNew.Count == 0)
            return Result.Fail<WorkflowActionResponse>(new NotFoundError(WorkflowErrors.NewAssigneeUnknown));

        var people = await users.GetByIdsIncludingInactiveAsync([userId, request.NewUserId], cancellationToken);
        var previous = people.First(user => user.Id == userId);

        var approved = workflow.ReplaceAssignee(
            actorId.Value,
            userId,
            request.NewUserId,
            WorkflowOutcomeTexts.AssigneeReplacedDescription(
                WorkflowsMapping.Describe(previous),
                WorkflowsMapping.Describe(activeNew[0])),
            DateTime.UtcNow);

        workflows.Update(workflow);

        if (await workflows.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail<WorkflowActionResponse>(WorkflowErrors.NothingPersisted);

        return Result.Ok(new WorkflowActionResponse(
            workflow.Status,
            approved ? WorkflowOutcomeTexts.AssigneeReplacedAndApproved : WorkflowOutcomeTexts.AssigneeReplaced));
    }
}
