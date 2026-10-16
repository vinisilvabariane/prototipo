using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class GetWorkflowUseCase(
    IWorkflowRepository workflows,
    IUserRepository users,
    ICurrentActor currentActor) : IGetWorkflowUseCase
{
    public async Task<Result<WorkflowDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<WorkflowDetailResponse>();

        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail<WorkflowDetailResponse>(new NotFoundError(WorkflowErrors.NotFound));

        var people = await WorkflowsMapping.PeopleAsync(users, workflow, cancellationToken);

        return Result.Ok(WorkflowsMapping.ToDetail(workflow, actorId.Value, people));
    }
}
