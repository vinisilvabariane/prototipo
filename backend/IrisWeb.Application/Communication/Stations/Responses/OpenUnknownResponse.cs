namespace IrisWeb.Application.Communication.Stations.Responses;

public sealed record OpenUnknownResponse(long UnknownId, string Workstation, string Line);
