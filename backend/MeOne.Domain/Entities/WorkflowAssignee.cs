namespace MeOne.Domain.Entities;

public sealed class WorkflowAssignee : Entity<ulong>
{
    private WorkflowAssignee() { }

    public ulong WorkflowId { get; private set; }

    public ulong UserId { get; private set; }

    public DateTime AssignedAt { get; private set; }

    internal static WorkflowAssignee Link(ulong userId, DateTime assignedAt) => new()
    {
        UserId = userId,
        AssignedAt = assignedAt,
    };
}
