namespace MeOne.Domain.Entities;

public sealed class OrgPosition : Entity<ulong>
{
    private OrgPosition() { }

    public string Title { get; private set; } = string.Empty;

    public int DepartmentId { get; private set; }

    public ulong? ParentId { get; private set; }

    public ulong? HolderId { get; private set; }

    public static OrgPosition Create(string title, int departmentId, ulong? parentId, ulong? holderId) => new()
    {
        Title = NormalizeTitle(title),
        DepartmentId = departmentId,
        ParentId = parentId,
        HolderId = holderId,
    };

    public bool Update(string title, int departmentId)
    {
        var normalized = NormalizeTitle(title);

        if (Title == normalized && DepartmentId == departmentId)
            return false;

        Title = normalized;
        DepartmentId = departmentId;
        return true;
    }

    public bool AssignHolder(ulong? userId)
    {
        if (HolderId == userId)
            return false;

        HolderId = userId;
        return true;
    }

    public bool MoveTo(ulong? parentId)
    {
        if (ParentId == parentId)
            return false;

        ParentId = parentId;
        return true;
    }

    public static string NormalizeTitle(string title) => title.Trim();
}
