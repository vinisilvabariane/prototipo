using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowActionResponse(WorkflowStatus Status, string Message);
