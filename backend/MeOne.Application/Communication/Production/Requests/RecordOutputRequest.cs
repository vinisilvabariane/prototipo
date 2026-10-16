namespace MeOne.Application.Communication.Production.Requests;

public sealed record RecordOutputRequest(ulong PostId, int Quantity, DateTime? HourStart);
