using IrisWeb.Domain.Enums;

namespace IrisWeb.Application.Communication.ModelCatalog.Responses;

public sealed record CatalogModelResponse(
    Guid Id,
    string Key,
    string Name,
    string? Description,
    AiTaskType TaskType,
    ModelPlatform Platform,
    string CreatedBy,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<CatalogModelVersionResponse> Versions,
    Guid? ActiveVersionId);
