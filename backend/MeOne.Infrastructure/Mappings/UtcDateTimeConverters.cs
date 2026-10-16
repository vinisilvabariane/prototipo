using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MeOne.Infrastructure.Mappings;

public sealed class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

public sealed class UtcNullableDateTimeConverter()
    : ValueConverter<DateTime?, DateTime?>(
        value => value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);
