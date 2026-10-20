namespace MeOne.Application.Communication.Helpdesk.Requests;

public sealed record OpenHelpdeskTicketRequest(ulong SectorId, IReadOnlyDictionary<string, string?>? Answers);
