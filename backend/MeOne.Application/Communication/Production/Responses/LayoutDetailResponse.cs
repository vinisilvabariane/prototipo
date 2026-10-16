namespace MeOne.Application.Communication.Production.Responses;

public sealed record LayoutDetailResponse(
    ulong Id,
    ulong LineId,
    string LineName,
    ulong ProductId,
    string ProductCode,
    string ProductName,
    ulong ShiftId,
    string ShiftName,
    IReadOnlyList<LayoutPostResponse> Posts,
    IReadOnlyList<LayoutDecorationResponse> Decorations,
    DateTime UpdatedAt,
    string UpdatedBy,
    bool CanManage);
