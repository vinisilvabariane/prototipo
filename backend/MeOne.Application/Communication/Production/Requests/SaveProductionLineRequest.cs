namespace MeOne.Application.Communication.Production.Requests;

public sealed record SaveProductionLineRequest(string Name, bool Active);
