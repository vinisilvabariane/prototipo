namespace MeOne.Application.Communication.Production.Requests;

public sealed record SaveProductionShiftRequest(string Name, TimeOnly StartTime, TimeOnly EndTime, bool Active);
