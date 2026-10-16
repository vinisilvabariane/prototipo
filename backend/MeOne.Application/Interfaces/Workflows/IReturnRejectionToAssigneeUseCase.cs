using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;

namespace MeOne.Application.Interfaces.Workflows;

public interface IReturnRejectionToAssigneeUseCase
{
    Task<Result> ExecuteAsync(
        ulong id,
        ulong stepId,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken);
}
