namespace IrisWeb.Domain.Entities;

public sealed class Role : Entity<ulong>
{
    private Role() { }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public static Role Create(string name, string description)
    {
        var role = new Role();
        role.Update(name, description);
        return role;
    }

    public bool Update(string name, string description)
    {
        var trimmedName = name.Trim();
        var trimmedDescription = description.Trim();

        if (Name == trimmedName && Description == trimmedDescription)
            return false;

        Name = trimmedName;
        Description = trimmedDescription;
        return true;
    }
}
