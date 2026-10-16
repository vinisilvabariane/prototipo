namespace MeOne.Application.Communication.Production.Requests;

public sealed record SaveProductionProductRequest(string Code, string Name, bool Active);
