using FluentResults;
using MeOne.Application.Communication.Workflows.Dtos;

namespace MeOne.Application.Interfaces.Workflows;

public interface IGetWorkflowAttachmentUseCase
{
    Task<Result<WorkflowAttachmentDownload>> ExecuteAsync(ulong id, ulong attachmentId, CancellationToken cancellationToken);
}
