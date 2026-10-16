namespace MeOne.Application.Communication.Production.Requests;

public sealed record SaveLayoutRequest(IReadOnlyList<SaveLayoutPostRequest> Posts, IReadOnlyList<SaveLayoutDecorationRequest> Decorations);
