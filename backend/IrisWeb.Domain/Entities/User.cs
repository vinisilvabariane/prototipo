namespace IrisWeb.Domain.Entities;

public sealed class User : Entity<ulong>
{
    private readonly List<UserRole> _roles = [];

    private readonly List<UserApplication> _applications = [];

    private User() { }

    public uint Registration { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    public DateTime ModifiedAt { get; private set; }

    public ulong ModifiedBy { get; private set; }

    public int DepartmentId { get; private set; }

    public Department? Department { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public IReadOnlyCollection<UserApplication> Applications => _applications;

    public IReadOnlyList<string> RoleNames =>
        [.. _roles.Where(link => link.Role is not null).Select(link => link.Role!.Name)];

    public IReadOnlyList<string> ApplicationNames =>
        [.. _applications
            .Where(link => link.Application is { Active: true })
            .Select(link => link.Application!.Name)];

    /// <summary>
    /// <paramref name="modifiedBy"/> é o id de quem cadastrou; 0 quando o próprio colaborador se
    /// cadastrou pela tela pública (não há usuário logado para culpar).
    /// </summary>
    public static User Create(
        uint registration,
        string name,
        string? email,
        string passwordHash,
        Department department,
        ulong modifiedBy)
    {
        var user = new User { PasswordHash = passwordHash };
        user.Update(registration, name, email, department, modifiedBy);
        return user;
    }

    public void Update(uint registration, string name, string? email, Department department, ulong modifiedBy)
    {
        Registration = registration;
        Name = name.Trim();
        Email = NormalizeEmail(email);
        Department = department;
        DepartmentId = department.Id;
        Touch(modifiedBy);
    }

    public void ChangePassword(string passwordHash, ulong modifiedBy)
    {
        PasswordHash = passwordHash;
        Touch(modifiedBy);
    }

    public void Deactivate(ulong modifiedBy)
    {
        Active = false;
        Touch(modifiedBy);
    }

    public bool HasRole(ulong roleId) => _roles.Any(link => link.RoleId == roleId);

    public bool HasRole(string roleName) => RoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase);

    public void GrantRole(Role role) => _roles.Add(UserRole.Link(Id, role));

    public bool RevokeRole(ulong roleId) => _roles.RemoveAll(link => link.RoleId == roleId) > 0;

    public static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private void Touch(ulong modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}
