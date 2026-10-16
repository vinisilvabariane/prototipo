using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowStepResponse(
    ulong Id,
    WorkflowAction Action,
    string Description,
    string? Justification,
    ulong UserId,
    string UserName,
    DateTime CreatedAt,
    ReviewStage Review,
    ReviewRole ActorRole);
