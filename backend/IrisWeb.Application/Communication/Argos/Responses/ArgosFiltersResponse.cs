namespace IrisWeb.Application.Communication.Argos.Responses;

public sealed record ArgosFiltersResponse(
    IReadOnlyList<string> Stations,
    IReadOnlyList<string> Setups);
