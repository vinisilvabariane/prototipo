namespace MeOne.Application.Communication.OrgChart.Responses;

public sealed record OrgPositionResponse(
    ulong Id,
    string Title,
    ulong? ParentId,
    int DepartmentId,
    string Department,
    OrgHolderResponse? Holder);
