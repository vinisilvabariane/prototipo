using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Requests;

public sealed record CreateWorkflowRequest(
    string Title,
    string? Description,
    WorkflowPriority Priority,
    WorkflowCategory? Category,
    DateOnly? DueDate,
    ulong[]? AssigneeIds);
