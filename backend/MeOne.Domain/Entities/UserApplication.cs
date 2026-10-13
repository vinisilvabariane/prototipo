namespace MeOne.Domain.Entities;

public sealed class UserApplication : Entity<ulong>
{
    private UserApplication() { }

    public ulong UserId { get; private set; }

    public ulong ApplicationId { get; private set; }

    public ClientApplication? Application { get; private set; }

    public static UserApplication Link(ulong userId, ClientApplication application) => new()
    {
        UserId = userId,
        ApplicationId = application.Id,
        Application = application,
    };
}
