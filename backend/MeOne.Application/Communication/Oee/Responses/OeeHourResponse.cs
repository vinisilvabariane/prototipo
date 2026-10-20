namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeHourResponse(DateTime HourStart, int Seconds, OeePillarsResponse Pillars, double Produced, double Theoretical);
