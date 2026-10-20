namespace MeOne.Application.Communication.Helpdesk.Responses;

public sealed record HelpdeskSectorSummaryResponse(
    ulong Id,
    string Name,
    string? Description,
    string ColorHex,
    string Icon,
    IReadOnlyList<string> Badges,
    bool Active,
    string? ExternalUrl,
    int FieldCount,
    bool CanAttend);

public sealed record HelpdeskSectorListResponse(IReadOnlyList<HelpdeskSectorSummaryResponse> Items, bool CanManage);

public sealed record HelpdeskSectorDetailResponse(
    ulong Id,
    string Name,
    string? Description,
    string ColorHex,
    string Icon,
    IReadOnlyList<string> Badges,
    bool Active,
    string? ExternalUrl,
    HelpdeskFormDto Form,
    HelpdeskFormDto ResolutionForm,
    IReadOnlyList<string> SummaryFields,
    IReadOnlyList<ulong> AttendantRoleIds,
    bool CanAttend,
    bool CanManage);
