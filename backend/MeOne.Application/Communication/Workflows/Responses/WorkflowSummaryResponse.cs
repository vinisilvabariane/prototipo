using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowSummaryResponse(
    ulong Id,
    string Title,
    string Description,
    WorkflowPriority Priority,
    WorkflowCategory? Category,
    WorkflowStatus Status,
    DateTime CreatedAt,
    DateOnly? DueDate,
    DateTime? CompletedAt);
