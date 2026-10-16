namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunAllocationResponse(ulong UserId, string UserName, uint Registration, DateTime AllocatedAt);
