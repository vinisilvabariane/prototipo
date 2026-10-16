using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowDetailResponse(
    ulong Id,
    string Title,
    string Description,
    WorkflowPriority Priority,
    WorkflowCategory? Category,
    WorkflowStatus Status,
    DateTime CreatedAt,
    DateOnly? DueDate,
    DateTime? CompletedAt,
    ulong CreatorId,
    string CreatorName,
    IReadOnlyList<WorkflowAssigneeResponse> Assignees,
    IReadOnlyList<WorkflowAttachmentResponse> Attachments,
    IReadOnlyList<WorkflowStepResponse> Steps,
    WorkflowPermissionsResponse Permissions);
