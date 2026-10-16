namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunPersonResponse(ulong UserId, string UserName, uint Registration, double Minutes, int Produced, int OpenPosts);
