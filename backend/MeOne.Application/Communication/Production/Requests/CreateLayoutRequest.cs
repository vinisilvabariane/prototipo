namespace MeOne.Application.Communication.Production.Requests;

public sealed record CreateLayoutRequest(ulong LineId, ulong ProductId, ulong ShiftId);
