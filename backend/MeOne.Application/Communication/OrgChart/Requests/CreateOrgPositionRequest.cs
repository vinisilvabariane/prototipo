namespace MeOne.Application.Communication.OrgChart.Requests;

public sealed record CreateOrgPositionRequest(string Title, int DepartmentId, ulong? ParentId, ulong? HolderId);
