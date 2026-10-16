namespace MeOne.Application.Communication.WorkInstructions.Requests;

public sealed record WorkInstructionsQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
}
