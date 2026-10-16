namespace MeOne.Application.Communication.Workflows.Dtos;

public sealed record WorkflowAttachmentDownload(Stream Content, string FileName, string ContentType);
