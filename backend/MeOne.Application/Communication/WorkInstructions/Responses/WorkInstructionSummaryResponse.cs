namespace MeOne.Application.Communication.WorkInstructions.Responses;

public sealed record WorkInstructionSummaryResponse(
    ulong Id,
    string Code,
    string Title,
    string DocumentNumber,
    string Product,
    string PreparedBy,
    string ApprovedBy,
    int StepCount,
    long PdfSize,
    DateTime CreatedAt,
    DateTime UpdatedAt);
