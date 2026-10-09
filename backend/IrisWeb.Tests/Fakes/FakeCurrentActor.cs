using IrisWeb.Application.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeCurrentActor(
    string? registration = "vinicius",
    ulong? userId = null,
    params string[] roles) : ICurrentActor
{
    public string? Registration { get; } = registration;

    public ulong? UserId { get; } = userId;

    public bool IsInRole(string role) => roles.Contains(role);

    public static FakeCurrentActor Anonymous() => new(registration: null);

    public static FakeCurrentActor Master(ulong userId = 1) => new("master", userId, "master");

    public static FakeCurrentActor Common(ulong userId) => new(userId.ToString(), userId);
}
