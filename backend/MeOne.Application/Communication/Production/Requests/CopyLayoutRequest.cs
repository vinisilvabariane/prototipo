namespace MeOne.Application.Communication.Production.Requests;

public sealed record CopyLayoutRequest(IReadOnlyList<LayoutTargetRequest> Targets);
