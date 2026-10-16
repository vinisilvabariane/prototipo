namespace MeOne.Application.Communication.Production.Responses;

public sealed record LayoutListResponse(IReadOnlyList<LayoutSummaryResponse> Items, bool CanManage);
