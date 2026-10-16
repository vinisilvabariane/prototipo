namespace MeOne.Application.Interfaces.Auth;

public interface IAccessProfileCache
{
    void Invalidate(ulong userId);

    void InvalidateAll();
}
