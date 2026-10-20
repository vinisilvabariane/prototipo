namespace MeOne.Application.Communication.Helpdesk.Requests;

public sealed record ResolveHelpdeskTicketRequest(IReadOnlyDictionary<string, string?>? Answers, string? Note);
