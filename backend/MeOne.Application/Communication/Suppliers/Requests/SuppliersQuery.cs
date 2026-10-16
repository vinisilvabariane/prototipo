using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Requests;

public sealed record SuppliersQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public SupplierOrigin? Origin { get; init; }

    public MultiBranch? MultiBranch { get; init; }

    public SupplierCriticality? Criticality { get; init; }

    public SupplierStatusFilter? Status { get; init; }
}
