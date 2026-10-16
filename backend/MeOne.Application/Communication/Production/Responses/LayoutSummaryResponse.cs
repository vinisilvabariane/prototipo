namespace MeOne.Application.Communication.Production.Responses;

public sealed record LayoutSummaryResponse(ulong Id, ulong LineId, string LineName, ulong ProductId, string ProductCode, string ProductName, ulong ShiftId, string ShiftName, int PostCount, DateTime UpdatedAt);
