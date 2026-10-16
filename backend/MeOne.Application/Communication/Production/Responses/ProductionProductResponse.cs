namespace MeOne.Application.Communication.Production.Responses;

public sealed record ProductionProductResponse(ulong Id, string Code, string Name, bool Active);
