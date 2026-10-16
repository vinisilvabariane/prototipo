namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowAssigneeResponse(ulong UserId, string Name, uint Registration, DateTime AssignedAt, bool Approved);
