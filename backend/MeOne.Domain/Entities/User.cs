using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

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

    public string SecurityStamp { get; private set; } = NewSecurityStamp();

    public int FailedLoginCount { get; private set; }

    public DateTime? LockedUntil { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public IReadOnlyCollection<UserApplication> Applications => _applications;

    public IReadOnlyList<string> RoleNames =>
        [.. _roles.Where(link => link.Role is not null).Select(link => link.Role!.Name).Distinct()];

    public IReadOnlyList<string> ApplicationNames =>
        [.. _applications
            .Where(link => link.Application is { Active: true })
            .Select(link => link.Application!.Name)];

    public bool IsAdministrator => _roles.Any(link => link.Role is { IsAdministrator: true } && link.ScopeId is null);

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
        FailedLoginCount = 0;
        LockedUntil = null;
        RotateSecurityStamp();
        Touch(modifiedBy);
    }

    public void Deactivate(ulong modifiedBy)
    {
        Active = false;
        RotateSecurityStamp();
        Touch(modifiedBy);
    }

    public void RotateSecurityStamp() => SecurityStamp = NewSecurityStamp();

    public bool IsLockedOut(DateTime nowUtc) => LockedUntil > nowUtc;

    public void RegisterFailedLogin(DateTime nowUtc)
    {
        FailedLoginCount++;

        if (FailedLoginCount < LoginLockoutPolicy.MaxFailedAttempts)
            return;

        FailedLoginCount = 0;
        LockedUntil = nowUtc.Add(LoginLockoutPolicy.LockoutDuration);
    }

    public bool RegisterSuccessfulLogin()
    {
        if (FailedLoginCount == 0 && LockedUntil is null)
            return false;

        FailedLoginCount = 0;
        LockedUntil = null;
        return true;
    }

    public bool HasRole(ulong roleId, ulong? scopeId) =>
        _roles.Any(link => link.RoleId == roleId && link.ScopeId == scopeId);

    public void GrantRole(Role role, Scope? scope) => _roles.Add(UserRole.Link(Id, role, scope));

    public bool RevokeRole(ulong roleId, ulong? scopeId) =>
        _roles.RemoveAll(link => link.RoleId == roleId && link.ScopeId == scopeId) > 0;

    public static string? NormalizeEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static string NewSecurityStamp() => Guid.NewGuid().ToString("N");

    private void Touch(ulong modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }
}
