namespace MeOne.Application.Communication.Production.Requests;

public sealed record LayoutTargetRequest(ulong LineId, ulong ProductId, ulong ShiftId);
