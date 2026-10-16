using FluentResults;
using MeOne.Application.Communication.Workflows.Dtos;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface ICreateWorkflowUseCase
{
    Task<Result<WorkflowSummaryResponse>> ExecuteAsync(
        CreateWorkflowRequest request,
        IReadOnlyList<WorkflowAttachmentUpload> attachments,
        CancellationToken cancellationToken);
}
