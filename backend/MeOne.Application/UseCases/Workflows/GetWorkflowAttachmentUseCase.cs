using FluentResults;
using MeOne.Application.Communication.Workflows.Dtos;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class GetWorkflowAttachmentUseCase(IWorkflowRepository workflows, IWorkflowFileStorage storage)
    : IGetWorkflowAttachmentUseCase
{
    public async Task<Result<WorkflowAttachmentDownload>> ExecuteAsync(
        ulong id,
        ulong attachmentId,
        CancellationToken cancellationToken)
    {
        var workflow = await workflows.GetByIdAsync(id, cancellationToken);
        if (workflow is null)
            return Result.Fail<WorkflowAttachmentDownload>(new NotFoundError(WorkflowErrors.NotFound));

        var attachment = workflow.Attachments.FirstOrDefault(candidate => candidate.Id == attachmentId);
        if (attachment is null)
            return Result.Fail<WorkflowAttachmentDownload>(new NotFoundError(WorkflowErrors.AttachmentNotFound));

        var content = await storage.OpenReadAsync(attachment.StoredName, cancellationToken);
        if (content is null)
            return Result.Fail<WorkflowAttachmentDownload>(new NotFoundError(WorkflowErrors.AttachmentNotFound));

        return Result.Ok(new WorkflowAttachmentDownload(content, attachment.OriginalName, attachment.ContentType));
    }
}
