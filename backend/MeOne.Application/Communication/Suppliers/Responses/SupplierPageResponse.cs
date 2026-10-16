namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierPageResponse(IReadOnlyList<SupplierSummaryResponse> Items, int TotalCount, int Page, int PageSize, bool CanRegister);
