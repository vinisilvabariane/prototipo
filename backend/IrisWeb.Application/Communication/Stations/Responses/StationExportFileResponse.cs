namespace IrisWeb.Application.Communication.Stations.Responses;

public sealed record StationExportFileResponse(Stream Content, string FileName);
