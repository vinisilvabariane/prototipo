using System.Globalization;
using MeOne.Application.Interfaces;
using MeOne.Domain.Entities;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Fakes;

public sealed class FakeCurrentActor(string? registration, ulong? userId, Func<AccessProfile?> profile) : ICurrentActor
{
    public string? Registration { get; } = registration;

    public ulong? UserId { get; } = userId;

    public Task<AccessProfile?> GetAccessProfileAsync(CancellationToken cancellationToken) =>
        Task.FromResult(profile());

    public static FakeCurrentActor Anonymous() => new(null, null, () => null);

    public static FakeCurrentActor For(User user) =>
        new(user.Registration.ToString(CultureInfo.InvariantCulture), user.Id, () => AccessProfile.From(user));

    public static FakeCurrentActor Administrator(ulong userId = 999) =>
        new(userId.ToString(CultureInfo.InvariantCulture), userId,
            () => new AccessProfile(userId, active: true, "stamp", isAdministrator: true, []));

    public static FakeCurrentActor WithPermissions(ulong userId, params string[] permissions) =>
        new(userId.ToString(CultureInfo.InvariantCulture), userId,
            () => new AccessProfile(userId, active: true, "stamp", isAdministrator: false,
                permissions.Select(permission => new AccessGrant(permission, null))));
}
