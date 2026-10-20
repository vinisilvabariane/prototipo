namespace MeOne.Application.Communication.Helpdesk.Responses;

public sealed record HelpdeskRoleResponse(ulong Id, string Name, string Description);

public sealed record HelpdeskPostResponse(ulong Id, string Name);

public sealed record HelpdeskLocationResponse(ulong LineId, string LineName, IReadOnlyList<HelpdeskPostResponse> Posts);
