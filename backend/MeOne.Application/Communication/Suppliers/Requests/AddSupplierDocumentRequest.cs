using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Requests;

public sealed record AddSupplierDocumentRequest(SupplierDocumentKind Kind, ulong? OwnerId, string? Version, DateOnly? ExpiresOn);
