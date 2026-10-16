namespace MeOne.Application.Communication.Workflows.Responses;

public sealed record WorkflowAttachmentResponse(ulong Id, string Name, string ContentType, long Size, DateTime UploadedAt);
