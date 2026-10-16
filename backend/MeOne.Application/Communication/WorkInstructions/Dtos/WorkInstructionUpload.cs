namespace MeOne.Application.Communication.WorkInstructions.Dtos;

public sealed record WorkInstructionUpload(string FileName, long Length, Stream Content);
