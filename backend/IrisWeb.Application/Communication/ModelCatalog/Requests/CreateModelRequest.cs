using IrisWeb.Domain.Enums;

namespace IrisWeb.Application.Communication.ModelCatalog.Requests;

public sealed record CreateModelRequest(
    string Key,
    string Name,
    string? Description,
    AiTaskType TaskType,
    ModelPlatform Platform);
