namespace MeOne.Application.Communication.OrgChart.Responses;

public sealed record OrgHolderResponse(ulong UserId, string Name, uint Registration, string? Email, bool Active);
