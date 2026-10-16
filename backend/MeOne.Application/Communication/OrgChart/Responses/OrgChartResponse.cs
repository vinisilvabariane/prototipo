namespace MeOne.Application.Communication.OrgChart.Responses;

public sealed record OrgChartResponse(IReadOnlyList<OrgPositionResponse> Positions, bool CanManage);
