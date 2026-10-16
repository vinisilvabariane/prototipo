using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierDocumentResponse(
    ulong Id,
    SupplierDocumentKind Kind,
    ulong? OwnerId,
    string Name,
    string ContentType,
    long Size,
    string? Version,
    DateOnly? ExpiresOn,
    bool Expired,
    DateTime UploadedAt);
