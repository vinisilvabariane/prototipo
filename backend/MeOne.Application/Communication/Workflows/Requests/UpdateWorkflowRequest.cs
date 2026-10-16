using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Requests;

public sealed record UpdateWorkflowRequest(
    string Title,
    string? Description,
    WorkflowPriority Priority,
    WorkflowCategory? Category);
