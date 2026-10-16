namespace MeOne.Application.Communication.Production.Requests;

public sealed record SetupQuery
{
    public bool IncludeInactive { get; init; }
}
