namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunPageResponse(IReadOnlyList<RunSummaryResponse> Items, int TotalCount, int Page, int PageSize, bool CanSupervise);
