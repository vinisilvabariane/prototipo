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

public sealed class AddWorkflowAssigneeUseCase(
    IWorkflowRepository workflows,
    IUserRepository users,
    ICurrentActor currentActor) : IAddWorkflowAssigneeUseCase
{
    public async Task<Result<WorkflowAssigneeResponse>> ExecuteAsync(
        ulong id,
        AddWorkflowAssigneeRequest request,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<WorkflowAssigneeResponse>();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail<WorkflowAssigneeResponse>(new NotFoundError(WorkflowErrors.NotFound));

        if (workflow.CheckAddAssignee(actorId.Value, request.UserId) is { } refusal)
            return WorkflowsMapping.Fail<WorkflowAssigneeResponse>(refusal);

        var found = await users.GetActiveByIdsAsync([request.UserId], cancellationToken);
        if (found.Count == 0)
            return Result.Fail<WorkflowAssigneeResponse>(new NotFoundError(WorkflowErrors.UserNotFound));

        var user = found[0];
        var now = DateTime.UtcNow;

        workflow.AddAssignee(
            actorId.Value,
            user.Id,
            WorkflowOutcomeTexts.AssigneeAddedDescription(WorkflowsMapping.Describe(user)),
            now);

        workflows.Update(workflow);

        return await workflows.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(new WorkflowAssigneeResponse(user.Id, user.Name, user.Registration, now, Approved: false))
            : Result.Fail<WorkflowAssigneeResponse>(WorkflowErrors.NothingPersisted);
    }
}
