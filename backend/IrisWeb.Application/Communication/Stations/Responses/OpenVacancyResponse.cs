namespace IrisWeb.Application.Communication.Stations.Responses;

public sealed record OpenVacancyResponse(long VacancyId, string Workstation, string Line);
