using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Requests;

public sealed record SaveSupplierRequest(
    string SapCode,
    string Name,
    string Branch,
    MultiBranch MultiBranch,
    SupplierOrigin Origin,
    string? TaxId,
    string? CorporateName,
    SupplierCriticality Criticality,
    string ZipCode,
    string District,
    string? Street,
    string? Number,
    string? ContactName,
    string? Phone,
    string? Email);
