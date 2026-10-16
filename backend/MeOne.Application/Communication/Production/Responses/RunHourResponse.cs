namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunHourResponse(DateTime HourStart, int Produced);
