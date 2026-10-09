namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncRecommendedCollaborator(
    int Id,
    string? Name,
    int Registration,
    string? Function,
    string? Level,
    int Score,
    int Position,
    int PriorityGroup,
    bool HasValidPostTraining,
    int AllocatedPostsCount,
    int? RemainingSlots);
