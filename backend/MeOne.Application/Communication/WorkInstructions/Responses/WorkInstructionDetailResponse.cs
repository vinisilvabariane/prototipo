namespace MeOne.Application.Communication.WorkInstructions.Responses;

public sealed record WorkInstructionDetailResponse(
    ulong Id,
    string Code,
    string Title,
    string DocumentNumber,
    string Product,
    string PreparedBy,
    string ApprovedBy,
    string RequiredPpe,
    string ProcessSpecifications,
    string? Notes,
    long PdfSize,
    string CreatedBy,
    DateTime CreatedAt,
    string UpdatedBy,
    DateTime UpdatedAt,
    IReadOnlyList<WorkInstructionStepResponse> Steps,
    bool CanManage);
