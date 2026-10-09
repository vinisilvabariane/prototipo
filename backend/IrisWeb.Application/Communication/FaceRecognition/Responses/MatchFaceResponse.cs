namespace IrisWeb.Application.Communication.FaceRecognition.Responses;

public sealed record MatchFaceResponse(string Label, string? Registration, float? Score, string? Candidate = null);
