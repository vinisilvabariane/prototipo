using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Requests;

public sealed record SaveSupplierMaterialRequest(string Sku, string? BusinessUnit, string? MaterialType, SupplierCriticality? Criticality, bool? HasSapSpecification);
