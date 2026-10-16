namespace MeOne.Application.Communication.WorkInstructions.Dtos;

public sealed record WorkInstructionDownload(Stream Content, string FileName, string ContentType);
