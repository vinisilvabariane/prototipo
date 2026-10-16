namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowMessageResponse(ulong Id, ulong UserId, string UserName, string Text, DateTime CreatedAt);
