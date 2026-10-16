namespace MeOne.Application.Communication.Production.Requests;

public sealed record CandidatesQuery
{
    public string? Search { get; init; }
}
