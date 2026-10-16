namespace MeOne.Application.Communication.WorkInstructions.Responses;

public sealed record WorkInstructionPageResponse(
    IReadOnlyList<WorkInstructionSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    bool CanManage);
