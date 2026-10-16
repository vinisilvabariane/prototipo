namespace MeOne.Application.Communication.Production.Responses;

public sealed record ProductionShiftResponse(ulong Id, string Name, TimeOnly StartTime, TimeOnly EndTime, bool Active);
