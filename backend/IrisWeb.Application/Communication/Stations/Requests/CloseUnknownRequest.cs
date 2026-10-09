namespace IrisWeb.Application.Communication.Stations.Requests;

public sealed record CloseUnknownRequest(int EndedSecondsAgo, string? CandidateRegistration, float? CandidateDistance);
