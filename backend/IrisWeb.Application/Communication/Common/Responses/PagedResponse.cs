namespace IrisWeb.Application.Communication.Common.Responses;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
