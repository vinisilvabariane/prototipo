namespace MeOne.Application.Communication.Helpdesk.Requests;

public sealed record SaveHelpdeskSectorRequest(
    string? Name,
    string? Description,
    string? ColorHex,
    string? Icon,
    IReadOnlyList<string>? Badges,
    bool Active,
    string? ExternalUrl,
    HelpdeskFormDto? Form,
    HelpdeskFormDto? ResolutionForm,
    IReadOnlyList<string>? SummaryFields,
    IReadOnlyList<ulong>? AttendantRoleIds);
