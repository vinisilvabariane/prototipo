using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class ListWorkflowUsersUseCase(IUserRepository users) : IListWorkflowUsersUseCase
{
    public async Task<Result<IReadOnlyList<WorkflowUserResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var active = await users.ListActiveAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<WorkflowUserResponse>>(
            [.. active.Select(user => new WorkflowUserResponse(user.Id, user.Registration, user.Name))]);
    }
}
