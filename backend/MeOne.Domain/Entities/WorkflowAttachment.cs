namespace MeOne.Domain.Entities;

public sealed class WorkflowAttachment : Entity<ulong>
{
    private WorkflowAttachment() { }

    public ulong WorkflowId { get; private set; }

    public string OriginalName { get; private set; } = string.Empty;

    public string StoredName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long Size { get; private set; }

    public DateTime UploadedAt { get; private set; }

    public static WorkflowAttachment Create(
        string originalName,
        string storedName,
        string contentType,
        long size,
        DateTime uploadedAt) => new()
    {
        OriginalName = originalName.Trim(),
        StoredName = storedName,
        ContentType = contentType.Trim(),
        Size = size,
        UploadedAt = uploadedAt,
    };
}
