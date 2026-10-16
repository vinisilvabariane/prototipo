namespace MeOne.Application.Communication.WorkInstructions.Requests;

public sealed record SaveWorkInstructionRequest(
    string Code,
    string Title,
    string DocumentNumber,
    string Product,
    string PreparedBy,
    string ApprovedBy,
    string RequiredPpe,
    string ProcessSpecifications,
    string? Notes,
    SaveWorkInstructionStep[]? Steps);
