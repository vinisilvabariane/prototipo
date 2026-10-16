namespace MeOne.Application.Communication.Production.Responses;

public sealed record ProductionLineResponse(ulong Id, string Name, bool Active, DateTime CreatedAt);
