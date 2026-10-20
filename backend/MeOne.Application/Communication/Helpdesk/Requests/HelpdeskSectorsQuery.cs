namespace MeOne.Application.Communication.Helpdesk.Requests;

public sealed record HelpdeskSectorsQuery
{
    public bool IncludeInactive { get; init; }
}
