namespace MeOne.Application.Communication.WorkInstructions.Responses;

public sealed record WorkInstructionStepResponse(ulong Id, int Position, string Text, bool HasImage);
