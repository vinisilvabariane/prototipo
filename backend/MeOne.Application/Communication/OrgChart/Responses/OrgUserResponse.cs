namespace MeOne.Application.Communication.OrgChart.Responses;

public sealed record OrgUserResponse(ulong Id, uint Registration, string Name, string Department);
