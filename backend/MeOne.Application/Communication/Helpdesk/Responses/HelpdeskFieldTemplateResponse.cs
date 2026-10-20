using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Helpdesk.Responses;

public sealed record HelpdeskFieldTemplateResponse(
    ulong Id,
    string Name,
    HelpdeskFieldType Type,
    string Label,
    bool Required,
    IReadOnlyList<HelpdeskOptionDto> Options);
