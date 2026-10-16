using System.Text.Json;
using System.Text.Json.Serialization;
using MeOne.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeOne.Infrastructure.Mappings;

public static class RunSnapshotJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string Serialize(RunSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Options);

    public static RunSnapshot Deserialize(string json) =>
        JsonSerializer.Deserialize<RunSnapshot>(json, Options) ?? RunSnapshot.Empty;

    public sealed class Converter() : ValueConverter<RunSnapshot, string>(
        snapshot => Serialize(snapshot),
        json => Deserialize(json));

    public sealed class Comparer() : ValueComparer<RunSnapshot>(
        (left, right) => ReferenceEquals(left, right) || (left != null && right != null && Serialize(left) == Serialize(right)),
        snapshot => Serialize(snapshot).GetHashCode(),
        snapshot => snapshot);
}
