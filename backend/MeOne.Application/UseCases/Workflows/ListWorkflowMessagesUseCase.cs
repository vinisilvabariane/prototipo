using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class ListWorkflowMessagesUseCase(
    IWorkflowRepository workflows,
    IUserRepository users,
    ICurrentActor currentActor) : IListWorkflowMessagesUseCase
{
    public async Task<Result<IReadOnlyList<WorkflowMessageResponse>>> ExecuteAsync(
        ulong id,
        ulong stepId,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<IReadOnlyList<WorkflowMessageResponse>>();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail<IReadOnlyList<WorkflowMessageResponse>>(new NotFoundError(WorkflowErrors.NotFound));

        if (workflow.FindStep(stepId) is null)
            return Result.Fail<IReadOnlyList<WorkflowMessageResponse>>(new NotFoundError(WorkflowErrors.StepNotFound));

        var people = await WorkflowsMapping.PeopleAsync(users, workflow, cancellationToken);

        return Result.Ok<IReadOnlyList<WorkflowMessageResponse>>(
        [
            .. workflow.Messages
                .Where(message => message.StepId == stepId)
                .OrderBy(message => message.Id)
                .Select(message => new WorkflowMessageResponse(
                    message.Id,
                    message.UserId,
                    WorkflowsMapping.NameOf(people, message.UserId),
                    message.Text,
                    message.CreatedAt)),
        ]);
    }
}
