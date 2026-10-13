namespace MeOne.Application.Interfaces;

public interface ICurrentActor
{

    string? Registration { get; }

    ulong? UserId { get; }

    bool IsInRole(string role);
}
