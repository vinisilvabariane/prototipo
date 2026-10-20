using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Helpdesk.Requests;

public sealed record SaveHelpdeskFieldTemplateRequest(
    string? Name,
    HelpdeskFieldType Type,
    string? Label,
    bool Required,
    IReadOnlyList<HelpdeskOptionDto>? Options);
