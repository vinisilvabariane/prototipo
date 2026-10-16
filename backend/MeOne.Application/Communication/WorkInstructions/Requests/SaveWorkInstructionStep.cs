namespace MeOne.Application.Communication.WorkInstructions.Requests;

public sealed record SaveWorkInstructionStep(ulong? Id, string Text, bool RemoveImage);
