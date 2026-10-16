namespace MeOne.Domain.Entities;

public sealed class WorkInstructionStep : Entity<ulong>
{
    private WorkInstructionStep() { }

    public ulong WorkInstructionId { get; private set; }

    public int Position { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public string? ImageStoredName { get; private set; }

    public string? ImageContentType { get; private set; }

    public bool HasImage => ImageStoredName is not null;

    public static WorkInstructionStep Create(string text, string? imageStoredName, string? imageContentType) => new()
    {
        Text = text.Trim(),
        ImageStoredName = imageStoredName,
        ImageContentType = imageStoredName is null ? null : imageContentType,
    };

    public bool Edit(string text)
    {
        var normalized = text.Trim();

        if (Text == normalized)
            return false;

        Text = normalized;
        return true;
    }

    public string? SetImage(string storedName, string contentType)
    {
        var previous = ImageStoredName;
        ImageStoredName = storedName;
        ImageContentType = contentType;
        return previous;
    }

    public string? ClearImage()
    {
        var previous = ImageStoredName;
        ImageStoredName = null;
        ImageContentType = null;
        return previous;
    }

    internal void Place(int position) => Position = position;
}
