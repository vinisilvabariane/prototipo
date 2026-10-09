namespace IrisWeb.Application.Communication.Stations.Responses;

public sealed record OpenPresenceResponse(long SessionId, string Workstation, string Line);
