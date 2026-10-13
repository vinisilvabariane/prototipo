using FluentResults;
using MeOne.Application.Communication.Workflow.Responses;

namespace MeOne.Application.Interfaces.Workflow;

public interface ICreateWorkflowUseCase
{
    Task<Result<WorkflowResponse>> ExecuteAsync();
}
