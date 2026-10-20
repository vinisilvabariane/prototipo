using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Helpdesk;

public sealed record HelpdeskOptionDto(string Value, string Label, string? Icon, string? Color);

public sealed record HelpdeskDependencyDto(string FieldId, IReadOnlyList<string>? Values);

public sealed record HelpdeskFieldDto(
    string Id,
    HelpdeskFieldType Type,
    string Label,
    bool Required,
    string? Placeholder,
    IReadOnlyList<HelpdeskOptionDto>? Options,
    HelpdeskDependencyDto? DependsOn,
    ulong? TemplateId);

public sealed record HelpdeskFormDto(IReadOnlyList<HelpdeskFieldDto>? Fields);
