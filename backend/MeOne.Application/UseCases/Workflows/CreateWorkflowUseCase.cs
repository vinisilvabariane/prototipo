using FluentResults;
using MeOne.Application.Communication.Workflows.Dtos;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class CreateWorkflowUseCase(
    IWorkflowRepository workflows,
    IUserRepository users,
    IWorkflowFileStorage storage,
    ICurrentActor currentActor) : ICreateWorkflowUseCase
{
    public async Task<Result<WorkflowSummaryResponse>> ExecuteAsync(
        CreateWorkflowRequest request,
        IReadOnlyList<WorkflowAttachmentUpload> attachments,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<WorkflowSummaryResponse>();

        var now = DateTime.UtcNow;

        if (request.DueDate is { } dueDate && dueDate < DateOnly.FromDateTime(now).AddDays(-1))
            return Result.Fail<WorkflowSummaryResponse>(WorkflowErrors.DueDateInPast);

        var invalidAttachment = ValidateAttachments(attachments);
        if (invalidAttachment is not null)
            return Result.Fail<WorkflowSummaryResponse>(invalidAttachment);

        var assigneeIds = (request.AssigneeIds ?? []).Distinct().ToList();
        var found = await users.GetActiveByIdsAsync(assigneeIds, cancellationToken);
        if (found.Count != assigneeIds.Count)
            return Result.Fail<WorkflowSummaryResponse>(WorkflowErrors.UnknownAssignees);

        var workflow = Workflow.Create(
            request.Title,
            request.Description ?? string.Empty,
            request.Priority,
            request.Category,
            request.DueDate,
            actorId.Value,
            assigneeIds,
            now);

        var storedNames = new List<string>();

        try
        {
            foreach (var upload in attachments)
            {
                var storedName = await storage.SaveAsync(upload.Content, ExtensionOf(upload.FileName), cancellationToken);
                storedNames.Add(storedName);

                workflow.Attach(WorkflowAttachment.Create(upload.FileName, storedName, upload.ContentType, upload.Length, now));
            }

            await workflows.AddAsync(workflow, cancellationToken);

            if (await workflows.SaveChangesAsync(cancellationToken) <= 0)
            {
                await DiscardAsync(storedNames);
                return Result.Fail<WorkflowSummaryResponse>(WorkflowErrors.NothingPersisted);
            }
        }
        catch
        {
            await DiscardAsync(storedNames);
            throw;
        }

        return Result.Ok(WorkflowsMapping.ToSummary(workflow));
    }

    private static string? ValidateAttachments(IReadOnlyList<WorkflowAttachmentUpload> attachments)
    {
        if (attachments.Count > WorkflowErrors.MaxAttachments)
            return WorkflowErrors.TooManyAttachments;

        foreach (var upload in attachments)
        {
            if (upload.Length <= 0)
                return WorkflowErrors.AttachmentEmpty;

            if (upload.Length > WorkflowErrors.MaxAttachmentBytes)
                return WorkflowErrors.AttachmentTooLarge(upload.FileName);

            if (upload.FileName.Length > WorkflowErrors.MaxAttachmentNameLength)
                return WorkflowErrors.AttachmentNameTooLong(upload.FileName);
        }

        return null;
    }

    private static string ExtensionOf(string fileName)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.');

        return extension.Length is > 0 and <= 10 && extension.All(char.IsAsciiLetterOrDigit)
            ? extension.ToLowerInvariant()
            : string.Empty;
    }

    private async Task DiscardAsync(IEnumerable<string> storedNames)
    {
        foreach (var storedName in storedNames)
            await storage.DeleteAsync(storedName, CancellationToken.None);
    }
}
