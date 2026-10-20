using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeOne.Infrastructure.Mappings;

public static class HelpdeskJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Deserialize<T>(string json, T fallback) =>
        string.IsNullOrWhiteSpace(json) ? fallback : JsonSerializer.Deserialize<T>(json, Options) ?? fallback;

    public sealed class Converter<T>(T fallback) : ValueConverter<T, string>(
        value => Serialize(value),
        json => Deserialize(json, fallback));

    public sealed class Comparer<T>() : ValueComparer<T>(
        (left, right) => ReferenceEquals(left, right) || (left != null && right != null && Serialize(left) == Serialize(right)),
        value => Serialize(value).GetHashCode(),
        value => value);
}
