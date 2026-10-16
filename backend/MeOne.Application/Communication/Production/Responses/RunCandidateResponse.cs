namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunCandidateResponse(ulong Id, string Name, uint Registration, string Department, int MachinePosts, bool HoldsCommonPost, string? BusyLine);
