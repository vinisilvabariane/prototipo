using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierMaterialResponse(ulong Id, string Sku, string? BusinessUnit, string? MaterialType, SupplierCriticality? Criticality, bool? HasSapSpecification);
