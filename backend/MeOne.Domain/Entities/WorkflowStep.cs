using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed class WorkflowStep : Entity<ulong>
{
    private WorkflowStep() { }

    public ulong WorkflowId { get; private set; }

    public ulong UserId { get; private set; }

    public WorkflowAction Action { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public string? Justification { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public ReviewStage Review { get; private set; }

    internal static WorkflowStep Record(
        ulong userId,
        WorkflowAction action,
        string description,
        string? justification,
        DateTime createdAt,
        ReviewStage review = ReviewStage.None) => new()
    {
        UserId = userId,
        Action = action,
        Description = description,
        Justification = string.IsNullOrWhiteSpace(justification) ? null : justification.Trim(),
        CreatedAt = createdAt,
        Review = review,
    };

    internal void SetReview(ReviewStage review) => Review = review;
}
