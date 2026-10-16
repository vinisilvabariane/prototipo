using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed class SupplierDocument : Entity<ulong>
{
    private SupplierDocument() { }

    public ulong SupplierId { get; private set; }

    public SupplierDocumentKind Kind { get; private set; }

    public ulong? OwnerId { get; private set; }

    public string OriginalName { get; private set; } = string.Empty;

    public string StoredName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long Size { get; private set; }

    public string? Version { get; private set; }

    public DateOnly? ExpiresOn { get; private set; }

    public DateTime UploadedAt { get; private set; }

    public ulong UploadedBy { get; private set; }

    internal static SupplierDocument Create(
        SupplierDocumentKind kind,
        ulong? ownerId,
        string originalName,
        string storedName,
        string contentType,
        long size,
        string? version,
        DateOnly? expiresOn,
        ulong uploadedBy,
        DateTime uploadedAt) => new()
    {
        Kind = kind,
        OwnerId = ownerId,
        OriginalName = originalName.Trim(),
        StoredName = storedName,
        ContentType = contentType,
        Size = size,
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim(),
        ExpiresOn = expiresOn,
        UploadedBy = uploadedBy,
        UploadedAt = uploadedAt,
    };
}
