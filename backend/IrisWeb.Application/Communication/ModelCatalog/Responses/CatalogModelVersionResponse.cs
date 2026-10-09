using System.Text.Json;
using IrisWeb.Domain.Enums;

namespace IrisWeb.Application.Communication.ModelCatalog.Responses;

public sealed record CatalogModelVersionResponse(
    Guid Id,
    string Version,
    string Runtime,
    ModelVersionStatus Status,
    long? ArtifactSize,
    string? Sha256,
    JsonElement? Manifest,
    string CreatedBy,
    string? ApprovedBy,
    DateTime CreatedAtUtc,
    DateTime? ApprovedAtUtc);
