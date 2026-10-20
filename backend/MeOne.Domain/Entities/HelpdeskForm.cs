using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed record HelpdeskOption(string Value, string Label, string? Icon, string? Color);

public sealed record HelpdeskDependency(string FieldId, IReadOnlyList<string> Values);

public sealed record HelpdeskField(
    string Id,
    HelpdeskFieldType Type,
    string Label,
    bool Required,
    string? Placeholder,
    IReadOnlyList<HelpdeskOption> Options,
    HelpdeskDependency? DependsOn,
    ulong? TemplateId);

public sealed record HelpdeskForm(IReadOnlyList<HelpdeskField> Fields)
{
    public static HelpdeskForm Empty { get; } = new([]);

    public bool IsEmpty => Fields.Count == 0;

    public HelpdeskField? Find(string id) => Fields.FirstOrDefault(field => field.Id == id);

    public HelpdeskField? OfType(HelpdeskFieldType type) => Fields.FirstOrDefault(field => field.Type == type);
}
