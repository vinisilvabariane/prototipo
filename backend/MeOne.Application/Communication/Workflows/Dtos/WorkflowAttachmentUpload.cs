namespace MeOne.Application.Communication.Workflows.Dtos;

public sealed record WorkflowAttachmentUpload(string FileName, string ContentType, long Length, Stream Content);
