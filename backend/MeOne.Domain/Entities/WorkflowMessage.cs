namespace MeOne.Domain.Entities;

public sealed class WorkflowMessage : Entity<ulong>
{
    private WorkflowMessage() { }

    public ulong WorkflowId { get; private set; }

    public ulong StepId { get; private set; }

    public ulong UserId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    internal static WorkflowMessage Write(ulong stepId, ulong userId, string text, DateTime createdAt) => new()
    {
        StepId = stepId,
        UserId = userId,
        Text = text.Trim(),
        CreatedAt = createdAt,
    };
}
